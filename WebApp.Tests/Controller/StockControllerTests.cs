using CodingCleanProject.Controllers;
using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Helpers;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using CodingCleanProject.Repository;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace WebApp.Tests.Controller
{
    public class StockControllerTests
    {
        private readonly AppStockController _controller;
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        public StockControllerTests()
        {
            //dependency
            _stockRepository = A.Fake<IStockRepository>();
            _mapper = A.Fake<IMapper>();
            _controller = new AppStockController(_stockRepository, _mapper);
        }
        [Fact]
        public async void StockControllerTests_GetAllStocks_stocksDto()
        {
            //Arrange
            var stockModel = new List<Stock>
            {
                new Stock { CompanyName = "Company1", Symbol = "C1", Purchase = 100, LastDiv = 10, Industry = "Tech" },
                new Stock { CompanyName = "Company2", Symbol = "C2", Purchase = 200, LastDiv = 15, Industry = "Finance" }
            };
            var stockDtos = A.Fake<List<StockDto>>();

            A.CallTo(() => _stockRepository.GetAllAsync()).Returns(Task.FromResult(stockModel));

            var fakeStockDtos = stockModel.Select(stock => new StockDto
            {
                CompanyName = stock.CompanyName,
                Symbol = stock.Symbol,
                Purchase = stock.Purchase,
                LastDiv = stock.LastDiv,
                Industry = stock.Industry
            }).ToList();

            A.CallTo(() => _mapper.StockMapper.ToStockDto(A<Stock>.Ignored)).ReturnsLazily((Stock s) => fakeStockDtos.FirstOrDefault(x => x.Symbol == s.Symbol));

            //Act
            var result = await _controller.GetAllStocks();

            //Assert
            result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<List<StockDto>>();
        }
        [Fact]
        public async void StockControllerTests_CreateStocks_ReturnOk()
        {
            //Arrange
            //var stockDto = new CreateStockDTO { CompanyName = "Company2", Symbol = "C1", Purchase = 100, LastDiv = 10, Industry = "Tech" };
            var stockDto = A.Fake<CreateStockDTO>();
            var stockModel = A.Fake<Stock>();
            var StockCreate = A.Fake<Stock>();
            A.CallTo(() => _mapper.StockMapper.FromCreateStockDto(stockDto)).Returns(StockCreate);
            A.CallTo(()=> _stockRepository.CreateStockAsync(stockModel)).Returns(StockCreate);
            //Act
            var result = await _controller.CreateStock(stockDto);
            //Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            result.Should().NotBeNull();
        }



    }
}
