using CodingCleanProject.Interfaces;

namespace CodingCleanProject.Mapper
{
    public class Mapperr : IMapper
    {
        public IStockMapper StockMapper { get; }
        public ICommentMapper CommentMapper { get; }

        public Mapperr(IStockMapper stockMapper, ICommentMapper commentMapper)
        {
            StockMapper = stockMapper ?? throw new ArgumentNullException(nameof(stockMapper));
            CommentMapper = commentMapper ?? throw new ArgumentNullException(nameof(commentMapper));
        }
    }
}
