using Microsoft.EntityFrameworkCore;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Repository;

namespace CodingCleanProject.Services
{
    public static class TokenServicee
    {
        public static void AddTokenService(this IServiceCollection services)
        {
            services.AddScoped<ITokenService, TokenService>();
        }
    }
}
