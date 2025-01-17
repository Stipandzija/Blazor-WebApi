using Shared.Dtos.Account;
using Shared.Dtos.RefreshToken;
using CodingCleanProject.Models;
using System.Security.Claims;

namespace CodingCleanProject.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetTokenPrincipal(string token);
        Task<LoginResponse> GenerateNewRefreshToken(RefreshTokenDTO refreshTokenDTO);
    }
}