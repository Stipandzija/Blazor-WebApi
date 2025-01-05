using CodingCleanProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using CodingCleanProject.Mapper;
using Microsoft.EntityFrameworkCore;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Helpers;

namespace CodingCleanProject.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class AppStockController : Controller
    {

        private readonly IStockRepository _stockRepository;
        private readonly QueryObject _queryObject;
        public AppStockController(IStockRepository stockRepository,QueryObject queryObject)
        {
            _stockRepository = stockRepository;
            _queryObject = queryObject;
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllStocks()
        {
            var stocksModel = await _stockRepository.GetAllAsync();
            var stocksDto = stocksModel.Select(StockMapper.toStockDto);
            if (stocksDto.IsNullOrEmpty())
                return NotFound();
            return Ok(stocksDto);
        }

        [HttpGet("GetWithQuery")]
        public async Task<IActionResult> GetStocks([FromQuery] QueryObject queryObject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stocksModel = await _stockRepository.GetAsync(queryObject);
            var stocksDto = stocksModel.Select(StockMapper.toStockDto);
            if(stocksDto.IsNullOrEmpty())
                return NotFound();
            return Ok(stocksDto);
        }
        [HttpGet]
        [Route("{Id:int}")]
        public async Task<IActionResult> GetStockByID([FromRoute] int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stocks = await _stockRepository.GetByIdAsync(Id);
            if (stocks == null)
                return NotFound();
            return Ok(stocks.toStockDto());

        }
        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] CreateStockDTO createStockDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var StockModel = createStockDTO.postStockDTO();
            var flag = _stockRepository.CreateStockAsync(StockModel);
            /***return GetStockByID(StockModel.Id);***/ // vraca 200 ok
            return CreatedAtAction(nameof(GetStockByID), new { id = StockModel.Id }, StockModel.toStockDto()); // vraca 201 - znaci created
        }
        [HttpPut]
        [Route("{Id:int}")]
        public async Task<IActionResult> UpdateStock([FromRoute] int Id, [FromBody] UpdateStockDto updateStockDto) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = await _stockRepository.UpdateStockAsync(Id,updateStockDto);
            if (stockModel == null)
                return NotFound();

            return Ok(stockModel.toStockDto());
        }
        [HttpDelete]
        [Route("{Id:int}")]
        public async Task<IActionResult> DeleteStock([FromRoute] int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = await _stockRepository.DeleteStockAsync(Id);
            if(stockModel == null)
                return NotFound();
            return NoContent();
        }
    }
}
