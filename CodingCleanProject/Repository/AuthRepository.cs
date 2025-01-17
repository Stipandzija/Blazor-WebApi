using Microsoft.AspNetCore.Identity;
using CodingCleanProject.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodingCleanProject.Interfaces;
using Shared.Dtos.Account;

namespace CodingCleanProject.Repository
{
    public class LoginRepository : ILoginService
    {
        private readonly UserManager<User> _userManager;

        public LoginRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindUserAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<bool> ValidateUserPasswordAsync(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task UpdateUserWithRefreshTokenAsync(User user, string refreshToken, DateTime expiry)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = expiry;
            await _userManager.UpdateAsync(user);
        }

    }
}
