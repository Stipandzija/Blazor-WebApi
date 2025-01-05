using CodingCleanProject.Interfaces;
using Microsoft.Identity.Client;
using System.Security.Cryptography;

namespace CodingCleanProject.Repository
{
    public class RefreshTokenRepository : IRefreshToken
    {
        public RefreshToken GenerateRefreshToken()
        {
            return new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                JwtId = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.UtcNow.AddDays(1)
            };
        }

    }
}
