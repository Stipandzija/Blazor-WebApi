using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CodingCleanProject.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using CodingCleanProject.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Azure.Core;
using CodingCleanProject.Dtos.RefreshToken;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;

namespace CodingCleanProject.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, ITokenService tokenService, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new User
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.EmailAddress
                };
                Console.WriteLine(registerDto.UserName);
                var createUser = await _userManager.CreateAsync(user, registerDto.Password);
                if (!createUser.Succeeded)
                    return StatusCode(500, createUser.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                    return StatusCode(500, roleResult.Errors);

                return Ok(new NewUserDTO
                {
                    UserName = user.UserName,
                    Email = user.Email,
                });

            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByNameAsync(loginDto.UserName);
                if (user == null)
                    return Unauthorized("Invalid username");

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                {
                    return Unauthorized("Invalid password");
                }
                var refreshToken = _tokenService.GenerateRefreshToken();

                var newAccessToken = _tokenService.CreateToken(user);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = DateTime.Now.AddMinutes(10);
                await _userManager.UpdateAsync(user);

                Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddDays(1),
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });

                return Ok(new LoginResponse
                {
                    IsLogged=true,
                    JwtToken = newAccessToken,
                    RefreshToken = refreshToken

                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        [Authorize]
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> GenereateNewRefreshToken(RefreshTokenDTO refreshTokenDTO) 
        {
            var loginresult = await _tokenService.GenerateNewRefreshToken(refreshTokenDTO);
            Response.Cookies.Append("RefreshToken", loginresult.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            if (loginresult.IsLogged)
            { 
                return Ok(loginresult);
            }
            return Unauthorized();
        }

        [HttpGet("my-claims")]
        [Authorize]
        public async Task<IActionResult> GetMyClaims()
        {
            try
            {
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized("Access token nije pronaden");
                }
                var principal = _tokenService.GetTokenPrincipal(accessToken);

                if (principal == null)
                {
                    return Unauthorized("Token nije validan");
                }

                var user = await _userManager.FindByNameAsync(principal.Identity.Name);

                if (user == null)
                {
                    return Unauthorized("Korisnik nije pronaden");
                }
                var claims = await _userManager.GetClaimsAsync(user);
                return Ok(new
                {
                    UserId = user.Id,
                    Claims = claims.Select(c => new { c.Type, c.Value })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("RefreshToken");
            HttpContext.SignOutAsync();

            return Ok("Logged out successfully");
        }
        [Authorize]
        [HttpGet("getUserDetails")]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var userName = User.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                    return Unauthorized("User is not logged in");

                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                    return NotFound("User not found");

                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);

                return Ok(new
                {
                    user.UserName,
                    user.Email,
                    Roles = roles,
                    Claims = claims.Select(c => new { c.Type, c.Value })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    }
}