using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos.Account
{
    public class RegisterDTO
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
