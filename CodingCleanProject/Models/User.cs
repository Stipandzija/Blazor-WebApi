using Microsoft.AspNetCore.Identity;

namespace CodingCleanProject.Models
{
    public class User : IdentityUser
    {

        public int Risk { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set;}
    }
}
