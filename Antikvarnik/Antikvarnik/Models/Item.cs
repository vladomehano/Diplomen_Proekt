
using System.ComponentModel.DataAnnotations;
using Antikvarnik.Models.Enums;

namespace Antikvarnik.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(100)]
        public string Condition { get; set; } = string.Empty;

        [StringLength(80)]
        public string? YearOrPeriod { get; set; }

        public ItemStatus Status { get; set; } = ItemStatus.Waiting;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public string? SellerId { get; set; }

        public AppUser? Seller { get; set; }

        public string? ReservedByUserId { get; set; }

        public AppUser? ReservedByUser { get; set; }

        public string? ApprovedByAdminId { get; set; }

        public AppUser? ApprovedByAdmin { get; set; }

        public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public string MainPicUrl => Images
            .OrderByDescending(i => i.IsMain)
            .ThenBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault() ?? "/images/placeholders/item-placeholder.svg";

        public string[] Pictures => Images
            .OrderBy(i => i.SortOrder)
            .Select(i => i.ImageUrl)
            .ToArray();
    }
}