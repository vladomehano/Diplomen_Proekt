using System.ComponentModel.DataAnnotations;

namespace Antikvarnik.Models
{
    public class OfferMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentOn { get; set; } = DateTime.UtcNow;

        public int OfferId { get; set; }

        public Offer Offer { get; set; } = null!;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        public AppUser Sender { get; set; } = null!;
    }
}