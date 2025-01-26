using CodingCleanProject.Data;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace CodingCleanProject.Repository
{
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly AppDbContext _appDbContext;
        public PortfolioRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<UserStock> CreateAsync(UserStock portfolio)
        {
            await _appDbContext.AddAsync(portfolio);
            await _appDbContext.SaveChangesAsync();
            return portfolio;
        }

        public async Task<UserStock?> DeleteAsync(User user,string symbol)
        {
            var portModel = await _appDbContext.Portofolios.FirstOrDefaultAsync(x=>x.UserId== user.Id && symbol.ToLower()==x.Stock.Symbol.ToLower());
            if (portModel != null)
            {
                return null;
            }
            _appDbContext.Portofolios.Remove(portModel);
            await _appDbContext.SaveChangesAsync();
            return portModel;
        }

        public async Task<List<Stock>> GetUserPortfolio(User user)
        {
            return await _appDbContext.Portofolios.Where(x => x.UserId == user.Id)
                .Select(stock => new Stock 
                {
                    Id = stock.StockId,
                    Symbol = stock.Stock.Symbol,
                    CompanyName = stock.Stock.CompanyName,
                    Purchase = stock.Stock.Purchase,
                    LastDiv = stock.Stock.LastDiv,
                    Industry = stock.Stock.Industry,
                    MarketCap = stock.Stock.MarketCap
                }).ToListAsync();//select je ekvivalentno mapu iterira kroz sve i radi promjene 
        }
    }
}
