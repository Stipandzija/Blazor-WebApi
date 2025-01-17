using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace Domain.Models
{
    public class User : IdentityUser
    {

        public int Risk { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set;}
    }
}
