using CodingCleanProject.Data;
using CodingCleanProject.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CodingCleanProject.Repository
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;

        public TokenService(AppDbContext context)
        {
            _context = context;
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

    }
}
