using System.ComponentModel.DataAnnotations;

namespace Antikvarnik.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; }

        public bool IsHidden { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;
    }
}