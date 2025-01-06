using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CodingCleanProject.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using CodingCleanProject.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodingCleanProject.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
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
                var createUser = await _userManager.CreateAsync(user, registerDto.Password);
                if (!createUser.Succeeded)
                    return StatusCode(500, createUser.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                    return StatusCode(500, roleResult.Errors);

                // generirat JWT token za korisnika
                var accessToken = _tokenService.CreateToken(user);

                // generiraj i spremi refresh token kao korisnički claim
                var refreshToken = Guid.NewGuid().ToString();
                var addClaimResult = await _userManager.AddClaimAsync(user, new Claim("RefreshToken", refreshToken));
                if (!addClaimResult.Succeeded)
                    return StatusCode(500, "Neuspjelo postravljanje refresh tokena");

                // Postavi refresh token u HttpOnly kolačić
                Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new NewUserDTO
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Token = accessToken
                });
                ;
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterDTO loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByNameAsync(loginDto.UserName);
                if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                    return Unauthorized("Invalid username or password");

                var accessToken = _tokenService.CreateToken(user);

                var refreshToken = Guid.NewGuid().ToString();
                await _tokenService.RevokeToken(refreshToken);

                // postavlja refresh token u HttpOnly
                Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new
                {
                    AccessToken = accessToken,
                    UserName = user.UserName
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized("nedostaje refresh roken");

                var users = await _userManager.Users.ToListAsync();
                var user = users.FirstOrDefault(u =>
                    _userManager.GetClaimsAsync(u).Result.Any(c => c.Type == "RefreshToken" && c.Value == refreshToken));
                if (user == null)
                    return Unauthorized("Krivi ili zastarjeli token");

                // generiraj novi access token
                var newAccessToken = _tokenService.CreateToken(user);

                // potrebno generirat novi refresh token i ažuriraj korisnički claim
                var newRefreshToken = Guid.NewGuid().ToString();
                var removeOldClaimResult = await _userManager.RemoveClaimAsync(user, new Claim("RefreshToken", refreshToken));
                var addNewClaimResult = await _userManager.AddClaimAsync(user, new Claim("RefreshToken", newRefreshToken));

                if (!removeOldClaimResult.Succeeded || !addNewClaimResult.Succeeded)
                    return StatusCode(500, "nemoguce updatat token");

                // azurirat refresh token u coockiu
                Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
                return Ok(new
                {
                    AccessToken = newAccessToken
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }



    }
}
