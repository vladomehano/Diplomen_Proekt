using Antikvarnik.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Antikvarnik.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {

        public DbSet<Item> Items { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
