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

        public AccountController(UserManager<User> userManager, ITokenService tokenService,SignInManager<User> signInManager)
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

                // Ako korisnik postoji i ima refresh token potrebno provjerit je li validan
                if (user == null)
                    return Unauthorized("Invalid username");

                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                if (!result.Succeeded)
                {
                    return Unauthorized("Invalid password");
                }

                // ukoliko je korisnik prijavlje potrebvno provjerite refresh token i generirat novi access token
                var refreshToken = Request.Cookies["RefreshToken"];
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var userClaims = await _userManager.GetClaimsAsync(user);
                    var storedRefreshToken = userClaims.FirstOrDefault(c => c.Type == "RefreshToken")?.Value;

                    if (storedRefreshToken == refreshToken)
                    {
                        // Ako je refresh token validan, kreiraj novi access token
                        var accessToken = _tokenService.CreateToken(user);

                        var newRefreshTokenClaim = Guid.NewGuid().ToString();
                        var oldClaim = userClaims.FirstOrDefault(c => c.Type == "RefreshToken");

                        if (oldClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, oldClaim);
                        }
                       
                        await _userManager.AddClaimAsync(user, new Claim("RefreshToken", newRefreshTokenClaim));

                        Response.Cookies.Append("RefreshToken", newRefreshTokenClaim, new CookieOptions
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
                    }
                    else
                    {
                        return Unauthorized("Invalid or expired refresh token.");
                    }
                }

                // ako nema refresh tokena potrebno je kreirat novi access token
                var newAccessToken = _tokenService.CreateToken(user);
                var newRefreshToken = Guid.NewGuid().ToString();

                await _userManager.AddClaimAsync(user, new Claim("RefreshToken", newRefreshToken));

                // postavljanje refresh tokena u coockie
                Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
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
                    Token = newAccessToken
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
