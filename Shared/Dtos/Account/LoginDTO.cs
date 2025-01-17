using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Account
{
    public class LoginDTO
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }

    }
}
