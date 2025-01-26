using System.ComponentModel.DataAnnotations.Schema;

namespace CodingCleanProject.Models
{
    [Table("Portfolios")]
    public class UserStock
    {
        public string UserId { get; set; }
        public int StockId { get; set; }
        public User User { get; set; }
        public Stock Stock { get; set;}
    }
}
