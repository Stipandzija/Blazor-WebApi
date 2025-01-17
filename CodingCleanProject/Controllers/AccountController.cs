using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CodingCleanProject.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using CodingCleanProject.Interfaces;
using System.Security.Claims;
using CodingCleanProject.Dtos.RefreshToken;
using Microsoft.AspNetCore.Authentication;

namespace CodingCleanProject.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly ILoginService _loginService;
        private readonly IRegisterRepository _registerRepository;

        public AccountController(ITokenService tokenService, ILoginService loginService, IRegisterRepository registerRepository)
        {
            _tokenService = tokenService;
            _loginService = loginService;
            _registerRepository = registerRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _registerRepository.IsUserNameTakenAsync(registerDto.UserName))
                return BadRequest("Username is already taken");

            if (await _registerRepository.IsEmailTakenAsync(registerDto.EmailAddress))
                return BadRequest("Email is already in use");

            var user = new User
            {
                UserName = registerDto.UserName,
                Email = registerDto.EmailAddress
            };

            var createUserResult = await _registerRepository.CreateUserAsync(user, registerDto.Password);
            if (!createUserResult.Succeeded)
                return StatusCode(500, createUserResult.Errors);

            var assignRoleResult = await _registerRepository.AssignRoleToUserAsync(user, "User");
            if (!assignRoleResult.Succeeded)
                return StatusCode(500, assignRoleResult.Errors);

            return Ok(new
            {
                UserName = user.UserName,
                Email = user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _loginService.FindUserAsync(loginDto.UserName);
            if (user == null)
                return Unauthorized("Invalid username");

            var isPasswordValid = await _loginService.ValidateUserPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
                return Unauthorized("Invalid password");

            var refreshToken = _tokenService.GenerateRefreshToken();
            var newAccessToken = _tokenService.CreateToken(user);

            await _loginService.UpdateUserWithRefreshTokenAsync(user, refreshToken, DateTime.Now.AddMinutes(10));

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(1),
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return Ok(new LoginResponse
            {
                IsLogged = true,
                JwtToken = newAccessToken,
                RefreshToken = refreshToken
            });
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
                return Ok(loginresult);

            return Unauthorized();
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("Access token nije pronaden, korisnik nije prijasvljen");
            }

            Response.Cookies.Delete("RefreshToken");
            HttpContext.SignOutAsync();

            return Ok("Logged out successfully");
        }
    }
}
