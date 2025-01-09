using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Dtos.Comment
{
    public class CommentDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreateOn { get; set; } = DateTime.Now;
        public DateTime UpdateOn { get; set; }
        public int? StockId { get; set; }
    }
}
