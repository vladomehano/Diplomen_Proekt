using System.ComponentModel.DataAnnotations;

namespace Antikvarnik.Models
{
    public class ItemImage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }

        public int SortOrder { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;
    }
}