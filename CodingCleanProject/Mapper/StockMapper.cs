using System.Runtime.CompilerServices;
using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Models;
using Microsoft.Identity.Client;

namespace CodingCleanProject.Mapper
{
    public static class StockMapper
    {
        public static StockDto toStockDto(this Stock stockModel)
        {
            return new StockDto
            {
                CompanyName = stockModel.CompanyName,
                Symbol = stockModel.Symbol,
                Purchase = stockModel.Purchase,
                LastDiv = stockModel.LastDiv,
                Industry = stockModel.Industry,
                Comments = stockModel.Comments.Select(x => x.ToCommentDto()).ToList()
            };
        }
        public static Stock postStockDTO(this CreateStockDTO stockModel)
        {
            return new Stock
            {
                CompanyName = stockModel.CompanyName,
                Symbol = stockModel.Symbol,
                Purchase = stockModel.Purchase,
                LastDiv = stockModel.LastDiv,
                Industry = stockModel.Industry
            };
        }

    }
}
