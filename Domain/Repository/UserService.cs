using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CodingCleanProject.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(User user)
        {
            return await _userManager.GetClaimsAsync(user);
        }
    }
}

