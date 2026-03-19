using System.ComponentModel.DataAnnotations;
using Antikvarnik.Models.Enums;

namespace Antikvarnik.Models
{
    public class Offer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal RequestedPrice { get; set; }

        public OfferStatus Status { get; set; } = OfferStatus.Waiting;

        [StringLength(1000)]
        public string? AdminComment { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;

        public ICollection<OfferMessage> Messages { get; set; } = new List<OfferMessage>();
    }
}