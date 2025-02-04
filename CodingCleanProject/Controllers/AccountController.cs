using CodingCleanProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodingCleanProject.Interfaces;
using Microsoft.AspNetCore.Authentication;
using CodingCleanProject.Dtos.Account;
using CodingCleanProject.Dtos.RefreshToken;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Cors;

namespace CodingCleanProject.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly ILoginService _loginService;
        private readonly IRegisterRepository _registerRepository;
        private readonly IAntiforgery _antiForgery;
        public AccountController(ITokenService tokenService, ILoginService loginService, IRegisterRepository registerRepository, IAntiforgery antiForgery)
        {
            _tokenService = tokenService;
            _loginService = loginService;
            _registerRepository = registerRepository;
            _antiForgery = antiForgery;
            
        }

        [HttpPost("register")]
        [ServiceFilter(typeof(ModelValidationAttribute))]

        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
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
        private async Task<bool> ValidateAntiForgeryToken()
        {
            try
            {
                await _antiForgery.ValidateRequestAsync(this.HttpContext);
                return true;
            }
            catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
            {
                return false;
            }
        }
    
        [HttpPost("login")]
        //ukoliko testiramo swagger moramo onemnogucit antiforgery
        [ServiceFilter(typeof(ModelValidationAttribute))]
        [ServiceFilter(typeof(IgnoreAntiforgeryTokenForSwagger))]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {


            Console.WriteLine("Login started");
            var user = await _loginService.FindUserAsync(loginDto.UserName);
            if (user == null)
            {
                Console.WriteLine("Invalid username");
                return Unauthorized("Invalid username");
            }

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
            var loginResponse = new LoginResponse
            {
                IsLogged = true,
                JwtToken = newAccessToken
            };
            loginResponse.SetRefreshToken(refreshToken);
            Console.WriteLine("Login finished");
            return Ok(loginResponse);
        }

        [Authorize]
        [HttpPost]
        [Route("RefreshToken")]
        [ServiceFilter(typeof(ModelValidationAttribute))]

        public async Task<IActionResult> GenereateNewRefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
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
        [Authorize]
        [HttpPost("logout")]
        [ServiceFilter(typeof(ModelValidationAttribute))]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized("Access token is missing, user not logged in");
            }

            Response.Cookies.Delete("RefreshToken");
            await HttpContext.SignOutAsync();

            return Ok("Logged out successfully");
        }
        [HttpGet("token")]
        [ServiceFilter(typeof(ModelValidationAttribute))]
        public IActionResult GetToken()
        {
            if (Request.Cookies.ContainsKey("XSRF-TOKEN"))
            {
                Response.Cookies.Delete("XSRF-TOKEN");
            }
            
            var tokens = _antiForgery.GetAndStoreTokens(HttpContext);

            Response.Cookies.Delete("XSRF-TOKEN");

            Response.Cookies.Append("XSRF-TOKEN", tokens.CookieToken!,
              new CookieOptions
              {
                  HttpOnly = false,
                  Secure = true,
                  SameSite = SameSiteMode.None,
                  Expires = DateTime.Now.AddMinutes(5)
              }); ;
            return Ok(new { token = tokens.RequestToken });
        }
        [HttpGet("test-exception")]
        public IActionResult TestException()
        {
            throw new Exception("Ovo je test glogal exeptiona");
        }


    }
}
