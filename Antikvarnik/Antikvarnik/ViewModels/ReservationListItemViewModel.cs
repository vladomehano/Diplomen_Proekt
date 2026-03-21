using Antikvarnik.Models.Enums;

namespace Antikvarnik.ViewModels
{
    public class ReservationListItemViewModel
    {
        public int ReservationId { get; set; }

        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string ItemImageUrl { get; set; } = string.Empty;

        public decimal ItemPrice { get; set; }

        public ItemStatus ItemStatus { get; set; }

        public ReservationStatus ReservationStatus { get; set; }

        public DateTime ReservedOn { get; set; }

        public DateTime? ProcessedOn { get; set; }

        public string UserEmail { get; set; } = string.Empty;
    }
}