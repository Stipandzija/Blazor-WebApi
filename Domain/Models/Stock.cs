﻿namespace Domain.Models
{
    public class Stock
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? CompanyName { get; set; }
        public decimal Purchase { get; set; }
        public decimal LastDiv { get; set; }
        public string? Industry { get; set; }

        public long MarketCap { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
