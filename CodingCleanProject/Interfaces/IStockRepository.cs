using CodingCleanProject.Models;
using Microsoft.EntityFrameworkCore.Update.Internal;
using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Helpers;
namespace CodingCleanProject.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> GetAllAsync();
        Task<List<Stock>> GetAsync(QueryObject queryObject);
        Task<Stock?> GetByIdAsync(int id);
        Task<Stock?> CreateStockAsync(Stock StockModel);
        Task<Stock?> UpdateStockAsync(int id, UpdateStockDto updateStockDto);
        Task<Stock?> DeleteStockAsync(int id);
        Task<bool> ExistStock(int Id);
        
    }
}
