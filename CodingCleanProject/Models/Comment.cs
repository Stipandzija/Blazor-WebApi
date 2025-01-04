namespace CodingCleanProject.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreateOn { get; set; } = DateTime.Now;
        public DateTime UpdateOn { get; set; }
      
        public int? StockId { get; set; }
        public Stock? Stock { get; set; }

    }
}
