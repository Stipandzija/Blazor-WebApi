using CodingCleanProject.Mapper;

namespace CodingCleanProject.Interfaces
{
    public interface IMapper
    {
        IStockMapper StockMapper { get; }
        ICommentMapper CommentMapper { get; }
    }
}
