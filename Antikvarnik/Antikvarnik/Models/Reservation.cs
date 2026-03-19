using Antikvarnik.Models.Enums;

namespace Antikvarnik.Models
{
    public class Reservation
    {
        public int Id { get; set; }

        public DateTime ReservedOn { get; set; } = DateTime.UtcNow;

        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        public DateTime? ProcessedOn { get; set; }

        public int ItemId { get; set; }

        public Item Item { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;

        public AppUser User { get; set; } = null!;

        public string? ProcessedByAdminId { get; set; }

        public AppUser? ProcessedByAdmin { get; set; }
    }
}