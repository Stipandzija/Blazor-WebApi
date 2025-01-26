using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCleanProject.Models
{
    [Table("Stocks")]
    public class Stock
    {
        public int Id { get; set; }
        public string? Symbol { get; set; }
        public string? CompanyName { get; set; }
        public decimal Purchase { get; set; }
        public decimal LastDiv { get; set; }
        public string? Industry { get; set; }

        public long MarketCap { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public List<UserStock> userStocks { get; set; } = new List<UserStock>();
    }
}
