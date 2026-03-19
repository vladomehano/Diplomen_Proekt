using Microsoft.AspNetCore.Identity;


namespace Antikvarnik.Models
{
    public class AppUser : IdentityUser
    {
        

        public ICollection<Item> OfferedItems { get; set; } = new List<Item>();

        public ICollection<Item> ApprovedItems { get; set; } = new List<Item>();

        public ICollection<Item> ReservedItems { get; set; } = new List<Item>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();

        public ICollection<OfferMessage> SentOfferMessages { get; set; } = new List<OfferMessage>();

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public ICollection<Reservation> ProcessedReservations { get; set; } = new List<Reservation>();
    }
}