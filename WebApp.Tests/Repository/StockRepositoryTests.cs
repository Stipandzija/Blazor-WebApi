using CodingCleanProject.Data;
using CodingCleanProject.Models;
using CodingCleanProject.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
namespace WebApp.Tests.Repository
{
    public class StockRepositoryTests
    {
        private async Task<AppDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var dbContext = new AppDbContext(options);
            dbContext.Database.EnsureCreated();
            if (await dbContext.stocks.CountAsync() <= 0)
            {
                for (int i = 0;i<10;i++) 
                {
                    dbContext.stocks.Add(
                        new Stock()
                        {
                            Symbol = $"SYM{i}",
                            CompanyName = $"Company {i}",
                            Purchase = Math.Round((decimal)(new Random().NextDouble() * 499 + 1), 2),
                            LastDiv = Math.Round((decimal)(new Random().NextDouble() * 49 + 1), 2),
                            Industry = i % 2 == 0 ? "Technology" : "Techonology2",
                            MarketCap = new Random().Next(1000000, 50000000),
                            Comments = new List<Comment>()
                            {
                                new Comment
                                {
                                    Title = $"Title {i}",
                                    Content = $"Comment for stock {i}",
                                    CreateOn = DateTime.Now.AddDays(-i),
                                    UpdateOn = DateTime.Now,
                                    AppUserId = $"User{i}"
                                }
                            }
                        }
                    );
                    await dbContext.SaveChangesAsync();
                }
            }
            return dbContext;
        }
        [Fact]
        public async void StockRepository_GetBySymbolAsync_ReturnStockorNull() 
        {
            //Arrange
            var name = "SYM1";
            var dbContext = await GetDatabaseContext();
            var stockRepository = new StockRepository(dbContext);
            //Act
            var results = await stockRepository.GetBySymbolAsync(name);
            //Assert
            results.Should().NotBeNull();
            results.Should().BeOfType<Stock>();
        }
        [Fact]
        public async void StockRepository_GetAllAsync_ReturnListStocks()
        {
            //Arrange
            var dbContext = await GetDatabaseContext();
            var stockRepository = new StockRepository(dbContext);
            //Act
            var results = await stockRepository.GetAllAsync();
            //Assert
            results.Should().BeOfType<List<Stock>>();
        }
        [Fact]
        public async void StockRepository_ExistStock_ReturnBool()
        {
            //Arrange
            int Id = 1;
            var dbContext = await GetDatabaseContext();
            var stockRepository = new StockRepository(dbContext);
            //Act
            var result = await stockRepository.ExistStock(Id);
            //Assert
            result.Should().BeTrue();

        }
    }
}
