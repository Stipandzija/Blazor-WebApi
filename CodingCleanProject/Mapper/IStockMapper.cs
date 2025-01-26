using CodingCleanProject.Dtos.Stock;
using CodingCleanProject.Models;

namespace CodingCleanProject.Mapper
{
    public interface IStockMapper
    {
        StockDto ToStockDto(Stock stock);
        Stock FromCreateStockDto(CreateStockDTO stockDto);
    }
}
