using Microsoft.AspNetCore.Identity;

namespace CodingCleanProject.Models
{
    public class User : IdentityUser
    {

        public int Risk { get; set; }
    }
}
