using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Antikvarnik.Models
{
    [PrimaryKey(nameof(UserId), nameof(ItemId))]
    public class Favorite
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}