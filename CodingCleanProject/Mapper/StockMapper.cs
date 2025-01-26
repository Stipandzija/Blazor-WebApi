using CodingCleanProject.Dtos.Comment;
using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Models;
using System.Xml.Linq;

namespace CodingCleanProject.Mapper
{
    public class StockMapper : IStockMapper
    {
        private readonly ICommentMapper _commentMapper;

        public StockMapper(ICommentMapper commentMapper)
        {
            _commentMapper = commentMapper;
        }

        public StockDto ToStockDto(Stock stockModel)
        {
            return new StockDto
            {
                CompanyName = stockModel.CompanyName,
                Symbol = stockModel.Symbol,
                Purchase = stockModel.Purchase,
                LastDiv = stockModel.LastDiv,
                Industry = stockModel.Industry,
                Comments = stockModel.Comments.Select(comment => _commentMapper.ToCommentDto(comment)).ToList()

            };
        }

        public Stock FromCreateStockDto(CreateStockDTO stockModel)
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
