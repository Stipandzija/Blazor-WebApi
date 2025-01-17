using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using CodingCleanProject.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Dtos.Stock;
using CodingCleanProject.Helpers;

namespace CodingCleanProject.Repository
{
    public class StockRepository : IStockRepository
    {
        private readonly AppDbContext _context;
        public StockRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<Stock?> CreateStockAsync(Stock StockModel)
        {
            await _context.stocks.AddAsync(StockModel);
            await _context.SaveChangesAsync();
            return StockModel;
        }

        public async Task<Stock?> DeleteStockAsync(int id)
        {
            var stockmodel = await _context.stocks.FirstOrDefaultAsync(x => x.Id == id);
            if (stockmodel != null)
            {
                _context.stocks.Remove(stockmodel);
                await _context.SaveChangesAsync();
                return stockmodel;
            }
            return null;
        }

        public async Task<bool> ExistStock(int Id)
        {
            var stockmodel = await _context.stocks.FirstOrDefaultAsync(x => x.Id == Id);
            if (stockmodel == null)
                return false;
            return true;
        }

        public async Task<List<Stock>> GetAllAsync()
        {
            return await _context.stocks.Include(c=>c.Comments).ToListAsync();
        }

        public async Task<List<Stock>> GetAsync(QueryObject queryObject)
        {
            var stocks = _context.stocks.Include(x => x.Comments).AsQueryable();
            if (!string.IsNullOrWhiteSpace(queryObject.CompanyName)) 
            {
                stocks = stocks.Where(s => s.CompanyName.Contains(queryObject.CompanyName)); 
            }
            if (!string.IsNullOrWhiteSpace(queryObject.Symbol))
            {
                stocks = stocks.Where(x => x.Symbol.Contains(queryObject.Symbol));
            }
            if (!string.IsNullOrWhiteSpace(queryObject.SortBy))
            {
                if (queryObject.SortBy == "Symbol")
                {
                    stocks = queryObject.IsDecending ? stocks.OrderByDescending( x => x.Symbol) : stocks.OrderBy( x => x.Symbol );
                }
                if (queryObject.SortBy == "CompanyName")
                {
                    stocks = queryObject.IsDecending ? stocks.OrderByDescending(x => x.CompanyName) : stocks.OrderBy(x => x.CompanyName);
                }
            }
            var skipNumber =(queryObject.PageNumber-1)*queryObject.PageSize;//koliko objekata prikazujemo po stranici

            return await stocks.Skip(skipNumber).Take(queryObject.PageSize).ToListAsync();
        }


        public async Task<Stock?> GetByIdAsync(int id)
        {
            var stockmodel = await _context.stocks.Include(c => c.Comments).FirstOrDefaultAsync(X=>X.Id==id);
            if (stockmodel != null)
            {
                return stockmodel;
            }
            return null;
        }

        public async Task<Stock?> UpdateStockAsync(int id , UpdateStockDto updateStockDto)
        {
            var stockModel = await _context.stocks.FirstOrDefaultAsync(X => X.Id == id);
            if (stockModel != null) {
                stockModel.MarketCap = updateStockDto.MarketCap;
                stockModel.Purchase = updateStockDto.Purchase;
                stockModel.Symbol = updateStockDto.Symbol;
                stockModel.CompanyName = updateStockDto.CompanyName;
                stockModel.Industry = updateStockDto.Industry;
                stockModel.LastDiv = updateStockDto.LastDiv;

                await _context.SaveChangesAsync();
                return stockModel;
            }
            return null;
        }
    }
}
