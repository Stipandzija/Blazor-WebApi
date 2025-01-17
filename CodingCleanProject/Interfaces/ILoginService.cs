using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface ILoginService
    {
        Task<User?> FindUserAsync(string userName);
        Task<bool> ValidateUserPasswordAsync(User user, string password);
        Task UpdateUserWithRefreshTokenAsync(User user, string refreshToken, DateTime expiry);
    }
}
