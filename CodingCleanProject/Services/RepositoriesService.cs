using CodingCleanProject.Interfaces;
using CodingCleanProject.Repository;

namespace CodingCleanProject.Services
{
    public static class RepositoriesService
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<ICommentRepository, ComnmentRepository>();
        }
    }
}
