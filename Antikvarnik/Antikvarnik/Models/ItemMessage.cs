using System.ComponentModel.DataAnnotations;

namespace Antikvarnik.Models
{
    public class ItemMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(1000)]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentOn { get; set; } = DateTime.UtcNow;

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        [Required]
        public string SenderId { get; set; } = string.Empty;

        public AppUser Sender { get; set; } = null!;
    }
}