using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CodingCleanProject.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using CodingCleanProject.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Azure.Core;

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

                var refreshToken = Request.Cookies["RefreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    var oldClaim = userClaims.FirstOrDefault(c => c.Type == "RefreshToken");
                    if (oldClaim != null)
                    {
                        await _userManager.RemoveClaimAsync(user, oldClaim);
                    }
                    Response.Cookies.Delete("RefreshToken");
                }
                Console.WriteLine("refreshToken",refreshToken);
                var newAccessToken = _tokenService.CreateToken(user);
                var newRefreshToken = Guid.NewGuid().ToString();
                Console.WriteLine("newRefreshToken", newRefreshToken);

                await _userManager.AddClaimAsync(user, new Claim("RefreshToken", newRefreshToken));
                Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(2)
                });

                var expiresIn = DateTime.UtcNow.AddHours(2);

                return Ok(new NewUserDTO
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = newAccessToken,
                    Expires = expiresIn
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("refresh")]
        [Authorize]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized("nedostaje refresh token");

                var users = await _userManager.Users.ToListAsync();
                var user = users.FirstOrDefault(u =>
                    _userManager.GetClaimsAsync(u).Result.Any(c => c.Type == "RefreshToken" && c.Value == refreshToken));
                if (user == null)
                    return Unauthorized("Krivi ili zastarjeli token");

                var newAccessToken = _tokenService.CreateToken(user);

                var newRefreshToken = Guid.NewGuid().ToString();

                // uklonit sve stare refresh tokene iz claimova korisnika
                var claims = await _userManager.GetClaimsAsync(user);
                var oldRefreshTokens = claims.Where(c => c.Type == "RefreshToken").ToList();

                foreach (var oldToken in oldRefreshTokens)
                {
                    var removeResult = await _userManager.RemoveClaimAsync(user, oldToken);
                    if (!removeResult.Succeeded)
                        return StatusCode(500, "Nemoguće ukloniti stari refresh token");
                }

                var addNewClaimResult = await _userManager.AddClaimAsync(user, new Claim("RefreshToken", newRefreshToken));
                if (!addNewClaimResult.Succeeded)
                    return StatusCode(500, "Nemoguće dodati novi refresh token");

                Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddSeconds(30)
                });

                return Ok(new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("my-claims")]
        [Authorize]
        public async Task<IActionResult> GetMyClaims()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("nedostaje refresh roken");

            var users = await _userManager.Users.ToListAsync();
            var user = users.FirstOrDefault(u =>
                _userManager.GetClaimsAsync(u).Result.Any(c => c.Type == "RefreshToken" && c.Value == refreshToken));

            if (user == null)
                return Unauthorized("Korisnik nije pronađen.");

            var claims = await _userManager.GetClaimsAsync(user);

            return Ok(new
            {
                UserId = user.Id,
                Claims = claims.Select(c => new { c.Type, c.Value })
            });
        }
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("RefreshToken");

            return Ok("Logged out successfully.");
        }
        [Authorize]
        [HttpGet("getUserDetails")]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var userName = User.Identity?.Name;

                if (string.IsNullOrEmpty(userName))
                    return Unauthorized("User is not logged in.");

                var user = await _userManager.FindByNameAsync(userName);

                if (user == null)
                    return NotFound("User not found.");

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