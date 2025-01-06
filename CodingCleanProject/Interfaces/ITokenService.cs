using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface ITokenService
    {
        public Task RevokeToken(string token);
        public Task<bool> IsTokenActive(string token);
        string CreateToken(User user);
    }
}