using CodingCleanProject.Models;

namespace CodingCleanProject.Interfaces
{
    public interface IPortfolioRepository
    {
        Task<List<Stock>> GetUserPortfolio(User user);
        Task<UserStock> CreateAsync(UserStock userStock);
        Task<UserStock?> DeleteAsync(User portfolio, string symbol);
    }
}
