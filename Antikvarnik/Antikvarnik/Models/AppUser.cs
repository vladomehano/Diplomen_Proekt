using Microsoft.AspNetCore.Identity;

namespace Antikvarnik.Models
{
    public class AppUser : IdentityUser
    {
        public int FavoriteNumber { get; set; }
    }
}
