using CodingCleanProject.Interfaces;
using CodingCleanProject.Mapper;
using CodingCleanProject.Repository;

namespace CodingCleanProject.Services
{
    public static class RepositoriesService
    {
        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<ILoginService, LoginRepository>();
            services.AddScoped<IRegisterRepository, RegisterRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICommentRepository, ComnmentRepository>();
            services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICommentMapper, CommentMapper>();
            services.AddScoped<IStockMapper, StockMapper>();
            services.AddScoped<IMapper, Mapperr>();

            services.AddScoped<IgnoreAntiforgeryTokenForSwagger>();



        }
    }
}
