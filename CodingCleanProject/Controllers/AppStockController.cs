using Microsoft.AspNetCore.Mvc;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Helpers;
using CodingCleanProject.Dtos.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace CodingCleanProject.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class AppStockController : Controller
    {
        private const string StockCacheKey = "stocks";
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;
        public AppStockController(IMemoryCache memoryCache,IStockRepository stockRepository, IMapper mapper)
        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _cache = memoryCache;
        }

        private bool IsValidModel() => ModelState.IsValid;

        [HttpPost]
        public async Task<IActionResult> CreateStock([FromBody] CreateStockDTO createStockDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = _mapper.StockMapper.FromCreateStockDto(createStockDTO);
            var created = await _stockRepository.CreateStockAsync(stockModel);

            if (created==null)
                return StatusCode(500, "Error occurred while creating the stock");

            return CreatedAtAction(nameof(GetStockByID), new { id = stockModel.Id }, _mapper.StockMapper.ToStockDto(stockModel));
        }

        [HttpGet("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAllStocks()
        {
            if (!IsValidModel())
                return BadRequest(ModelState);
            if (_cache.TryGetValue(StockCacheKey, out IEnumerable<StockDto>? result))
            {
                return Ok(result);
            }
            var stocksModel = await _stockRepository.GetAllAsync();
            var stocksDto = stocksModel.Select(stock => _mapper.StockMapper.ToStockDto(stock)).ToList();

            if (!stocksDto.Any())
                return NotFound();

            return Ok(stocksDto);
        }

        [HttpGet("GetWithQuery")]
        public async Task<IActionResult> GetStocks([FromQuery] QueryObject queryObject)
        {
            if (!IsValidModel())
                return BadRequest(ModelState);

            var stocksModel = await _stockRepository.GetAsync(queryObject);
            var stocksDto = stocksModel.Select(stock => _mapper.StockMapper.ToStockDto(stock)).ToList();

            if (!stocksDto.Any())
                return NotFound();

            return Ok(stocksDto);
        }

        [HttpGet("{Id:int}")]
        public async Task<IActionResult> GetStockByID([FromRoute] int Id)
        {
            if (!IsValidModel())
                return BadRequest(ModelState);

            var stock = await _stockRepository.GetByIdAsync(Id);
            if (stock == null)
                return NotFound();

            return Ok(_mapper.StockMapper.ToStockDto(stock));
        }


        [HttpPut("{Id:int}")]
        public async Task<IActionResult> UpdateStock([FromRoute] int Id, [FromBody] UpdateStockDto updateStockDto)
        {
            if (!IsValidModel())
                return BadRequest(ModelState);

            var stockModel = await _stockRepository.UpdateStockAsync(Id, updateStockDto);
            if (stockModel == null)
                return NotFound();

            return Ok(_mapper.StockMapper.ToStockDto(stockModel));
        }

        [HttpDelete("{Id:int}")]
        public async Task<IActionResult> DeleteStock([FromRoute] int Id)
        {
            if (!IsValidModel())
                return BadRequest(ModelState);

            var stockModel = await _stockRepository.DeleteStockAsync(Id);
            if (stockModel == null)
                return NotFound();

            return NoContent();
        }
    }
}
