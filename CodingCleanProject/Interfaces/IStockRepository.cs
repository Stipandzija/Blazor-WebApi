using CodingCleanProject.Models;
using CodingCleanProject.Helpers;
using CodingCleanProject.Dtos.Stock;
namespace CodingCleanProject.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> GetAllAsync();
        Task<List<Stock>> GetAsync(QueryObject queryObject);
        Task<Stock?> GetByIdAsync(int id);
        Task<Stock?> GetBySymbolAsync(string symbol);
        Task<Stock?> CreateStockAsync(Stock StockModel);
        Task<Stock?> UpdateStockAsync(int id, UpdateStockDto updateStockDto);
        Task<Stock?> DeleteStockAsync(int id);
        Task<bool> ExistStock(int Id);
        
    }
}
