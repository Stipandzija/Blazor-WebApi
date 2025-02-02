﻿using System.ComponentModel.DataAnnotations;

namespace CodingCleanProject.Dtos.Stock
{
    public class UpdateStockDto
    {
        [Required]
        public string? Symbol { get; set; }
        public string? CompanyName { get; set; }
        public decimal Purchase { get; set; }
        public decimal LastDiv { get; set; }
        public string? Industry { get; set; }
        public long MarketCap { get; set; }
    }
}
