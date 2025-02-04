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
        private readonly string StockCacheKey = "stocks";
        private readonly IStockRepository _stockRepository;
        private readonly IMapper _mapper;
        private IMemoryCache _cache;
        private readonly ILogger<AppStockController> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
        public AppStockController(ILogger<AppStockController> logger, IMemoryCache memoryCache, IStockRepository stockRepository, IMapper mapper)

        {
            _stockRepository = stockRepository;
            _mapper = mapper;
            _cache = memoryCache;
            _logger = logger;
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
        //[ServiceFilter(typeof(ModelValidationAttribute))]
        public async Task<IActionResult> GetAllStocks()
        {
            string userId = User.Identity?.Name;
            string userStockCacheKey = $"{StockCacheKey}_{userId}";
            if (_cache.TryGetValue(userStockCacheKey, out List<StockDto?>? stocksDto))
            {
                _logger.LogInformation("Stock found in casche.");
            }
            else
            {
                try
                {
                    await _semaphore.WaitAsync();

                    if (_cache.TryGetValue(userStockCacheKey, out stocksDto))
                    {
                        _logger.LogInformation("Stock found in casche");
                    }
                    else 
                    {
                        _logger.LogInformation("Stock not found in cache. Fetching from db");

                        var stocksModel = await _stockRepository.GetAllAsync();
                        stocksDto = stocksModel.Select(stock => _mapper.StockMapper.ToStockDto(stock)).ToList();

                        if (!stocksDto.Any())
                            return NotFound();
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromSeconds(60))//koliko je aktivna u cachu, problem ako pristupamo cachu nikad nece nestat
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1))//rjesavamo problem slidingexpiirtion istice nakon 1 sata sswigurno
                            .SetPriority(CacheItemPriority.Normal)
                            .SetSize(1);
                        _cache.Set(userStockCacheKey, stocksDto, cacheEntryOptions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching stocks");
                    return StatusCode(500, "Internal server error");
                }
                finally 
                { 
                    _semaphore.Release(); 
                }

            }
            return Ok(stocksDto);
        }

        [HttpPost("ClearCache")]
        [Authorize]
        public async Task<IActionResult> ClearCache()
        {
            string userId = User.Identity?.Name;
            string userStockCacheKey = $"{StockCacheKey}_{userId}";
            _cache.Remove(userStockCacheKey);
            return Ok();
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
