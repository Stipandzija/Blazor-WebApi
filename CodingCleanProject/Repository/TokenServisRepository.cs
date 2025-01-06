using CodingCleanProject.Data;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CodingCleanProject.Repository
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _symmetricSecurityKey;

        public TokenService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
            _symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"]));
        }

        public async Task RevokeToken(string token)
        {
            var revokedToken = new RefreshToken
            {
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(1)
            };
            await _context.RefreshToken.AddAsync(revokedToken);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsTokenActive(string token)
        {
            // Provjera da li je token opozvan
            return !await _context.RefreshToken.AnyAsync(t => t.Token == token);
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName,user.UserName)
            };
            var creds = new SigningCredentials(_symmetricSecurityKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDes = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _configuration["JWT:Issuer"],
                Audience = _configuration["JWT:Audience"]
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDes);
            return tokenHandler.WriteToken(token);
        }
    }
}
