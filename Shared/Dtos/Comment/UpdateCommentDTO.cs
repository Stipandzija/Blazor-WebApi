using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Comment
{
    public class UpdateCommentDTO
    {
        [Required]
        [MinLength(5, ErrorMessage = "Title must be min of 5 char")]
        [MaxLength(256, ErrorMessage = "Previse karaktera")]
        public string Title { get; set; } = string.Empty;
        [Required]
        [MinLength(1, ErrorMessage = "Content must be min of 5 char")]
        [MaxLength(256, ErrorMessage = "Content karaktera")]
        public string Content { get; set; } = string.Empty;
        public DateTime UpdateOn { get; set; } = DateTime.Now;
    }
}
