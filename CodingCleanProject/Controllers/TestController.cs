using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace CodingCleanProject.Controllers
{
    [Route("api/test")]
    [ApiController]
    [Authorize]
    public class TestController : Controller
    {
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
                    timeLeftInSeconds = timeLeft.TotalSeconds,
                    timeLeft = timeLeft.ToString(@"hh\:mm\:ss")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
    }
}
