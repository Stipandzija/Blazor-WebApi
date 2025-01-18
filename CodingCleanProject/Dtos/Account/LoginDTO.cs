using System.ComponentModel.DataAnnotations;

namespace CodingCleanProject.Dtos.Account
{
    public class LoginDTO
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
