using CodingCleanProject.Extensions.ClaimsExtenions;
using CodingCleanProject.Interfaces;
using CodingCleanProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CodingCleanProject.Controllers
{
    [Route("api/portfolio")]
    [ApiController]
    public class PortfolioController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IStockRepository _stockRepository;
        private readonly IPortfolioRepository _portfolioRepository;
        public PortfolioController(IPortfolioRepository portfolioRepository ,UserManager<User> userManager, IStockRepository stockRepository)
        {
            _userManager = userManager;
            _stockRepository = stockRepository; 
            _portfolioRepository = portfolioRepository;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPortfolio() 
        {
            var UserName = User.GetUsername(); // user dolazi iz controlerbasa i vraca sve vezano za usera
            var AppUser = await _userManager.FindByNameAsync(UserName);
            var UserPortfolio = await _portfolioRepository.GetUserPortfolio(AppUser);

            return Ok(UserPortfolio);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddPortfolio(string symbol) 
        {
            var UserName = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(UserName);
            var stock = await _stockRepository.GetBySymbolAsync(symbol);

            if (stock == null)
                return BadRequest("Stock not fouind");
            var UserPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);
            if (UserPortfolio.Any(e => e.Symbol.ToLower() == symbol.ToLower()))
                return BadRequest("Cannot add same stock already exist");
            var PortfolioModel = new UserStock
            {
                StockId = stock.Id,
                UserId = appUser.Id
            };
            await _portfolioRepository.CreateAsync(PortfolioModel);
            if (PortfolioModel == null)
            {
                return StatusCode(500, "Cannot Create Portfolio");
            }
            return Created();
        }
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeletePortfolio(string symbol) 
        {
            var UserName = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(UserName);
            var UserPortfolio = await _portfolioRepository.GetUserPortfolio(appUser);
            var filteredDtock = UserPortfolio.Where(e => e.Symbol.ToLower() == symbol.ToLower());
            if (filteredDtock.Count()==1)
                await _portfolioRepository.DeleteAsync(appUser, symbol);
            else
                return BadRequest("Stock not in portfolio");
            return Ok();
        }
    }
}
