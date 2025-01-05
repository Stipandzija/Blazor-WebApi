using System.ComponentModel.DataAnnotations;

namespace CodingCleanProject.Dtos.Comment
{
    public class CreateCommentDTO
    {
        [Required]
        [MinLength(5,ErrorMessage ="Title must be min of 5 char")]
        [MaxLength(256,ErrorMessage ="Previse karaktera")]
        public string Title { get; set; } = string.Empty;
        [Required]
        [MinLength(1, ErrorMessage = "Content must be min of 5 char")]
        [MaxLength(256, ErrorMessage = "Content karaktera")]
        public string Content { get; set; } = string.Empty;
    }
}
