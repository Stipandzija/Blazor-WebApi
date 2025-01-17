﻿using System.ComponentModel.DataAnnotations;

namespace CodingCleanProject.Dtos.Stock
{
    public class CreateStockDTO
    {
        [Required]
        public string? Symbol { get; set; }
        [Required]
        public string? CompanyName { get; set; }
        [Required]
        public decimal Purchase { get; set; }
        [Required]
        public decimal LastDiv { get; set; }
        [Required]
        public string? Industry { get; set; }
        [Required]
        public long MarketCap { get; set; }
    }
}
