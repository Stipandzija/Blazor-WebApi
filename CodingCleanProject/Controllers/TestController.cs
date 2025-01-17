using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using CodingCleanProject.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CodingCleanProject.Controllers
{
    [Route("api/test")]
    [ApiController]
    [Authorize]
    public class TestController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ILoginService _loginService;
        public TestController(IUserRepository userRepository, ITokenService tokenService, ILoginService loginService)
        {

            _userRepository = userRepository;
            _tokenService = tokenService;
            _loginService = loginService;
        }


        [HttpGet]
        [Route("Dozvola")]
        public IActionResult GetTokenTimeLeft()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Token nije pronađen u headeru");
                }
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim == null)
                {
                    return Unauthorized();
                }
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value)).UtcDateTime;

                var timeLeft = expirationTime - DateTime.UtcNow;

                if (timeLeft.TotalSeconds <= 0)
                {
                    return Unauthorized("Token je istekao");
                }

                return Ok(new
                {
                    timeLeft = timeLeft.ToString(@"hh\:mm\:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
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

                var user = await _loginService.FindUserAsync(principal.Identity.Name);
                if (user == null)
                {
                    return Unauthorized("Korisnik nije pronaden");
                }
                var claims = await _userRepository.GetUserClaimsAsync(user);
                return Ok(new
                {
                    UserId = user.Id,
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
