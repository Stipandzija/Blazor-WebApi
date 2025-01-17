using CodingCleanProject.Models;
using Microsoft.AspNetCore.Identity;

namespace CodingCleanProject.Repository
{
    public class RegisterRepository
    {
        private readonly UserManager<User> _userManager;

        public RegisterRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> IsUserNameTakenAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }

        public async Task<bool> IsEmailTakenAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> AssignRoleToUserAsync(User user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return (result.Succeeded, result.Errors.Select(e => e.Description));
        }
    }
}
