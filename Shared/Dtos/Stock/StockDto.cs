﻿using Shared.Dtos.Comment;

namespace Shared.Dtos.Stock
{
    public class StockDto
    {
        //public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? CompanyName { get; set; }
        public decimal Purchase { get; set; }
        public decimal LastDiv { get; set; }
        public string? Industry { get; set; }
        public long MarketCap { get; set; }

        public ICollection<CommentDto> Comments { get; set; }
        //public long MarketCap { get; set; }
    }
}
