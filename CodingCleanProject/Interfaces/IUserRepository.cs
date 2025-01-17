using CodingCleanProject.Models;
using System.Security.Claims;

namespace CodingCleanProject.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<Claim>> GetUserClaimsAsync(User user);
    }
}
