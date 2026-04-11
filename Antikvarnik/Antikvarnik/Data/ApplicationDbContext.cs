using Antikvarnik.Models;
using Antikvarnik.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Antikvarnik.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<ItemImage> ItemImages { get; set; }

        public DbSet<Review> Reviews { get; set; }

        

        
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ItemMessage> ItemMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Item>()
                .Property(i => i.Status)
                .HasConversion<string>();

            builder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>()
                .HasOne(i => i.Seller)
                .WithMany(u => u.OfferedItems)
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>()
                .HasOne(i => i.ReservedByUser)
                .WithMany(u => u.ReservedItems)
                .HasForeignKey(i => i.ReservedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Item>()
                .HasOne(i => i.ApprovedByAdmin)
                .WithMany(u => u.ApprovedItems)
                .HasForeignKey(i => i.ApprovedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ItemImage>()
                .HasOne(ii => ii.Item)
                .WithMany(i => i.Images)
                .HasForeignKey(ii => ii.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItemMessage>()
                .HasOne(im => im.Item)
                .WithMany(i => i.Messages)
                .HasForeignKey(im => im.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItemMessage>()
                .HasOne(im => im.Sender)
                .WithMany(u => u.SentItemMessages)
                .HasForeignKey(im => im.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- NEW: Configure Offer / OfferMessage to avoid multiple cascade paths ---
            builder.Entity<Offer>()
                .HasOne(o => o.User)
                .WithMany(u => u.Offers)
                .HasForeignKey(o => o.UserId)
                // prevent cascade from User -> Offer -> OfferMessage and also User -> OfferMessage
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OfferMessage>()
                .HasOne(om => om.Offer)
                .WithMany(o => o.Messages)
                .HasForeignKey(om => om.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OfferMessage>()
                .HasOne(om => om.Sender)
                .WithMany(u => u.SentOfferMessages)
                .HasForeignKey(om => om.SenderId)
                // keep sender -> offerMessage from cascading to avoid multiple cascade paths
                .OnDelete(DeleteBehavior.Restrict);
            // ----------------------------------------------------------------------

            builder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.ItemId });

            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Item)
                .WithMany(i => i.FavoritedByUsers)
                .HasForeignKey(f => f.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Монети", Description = "Редки монети и нумизматични ценности." },
                new Category { Id = 2, Name = "Часовници", Description = "Ретро и колекционерски часовници." },
                new Category { Id = 3, Name = "Книги", Description = "Антикварни книги, албуми и издания." },
                new Category { Id = 4, Name = "Порцелан", Description = "Порцеланови сервизи, чаши и фигурки." },
                new Category { Id = 5, Name = "Военни артефакти", Description = "Исторически военни предмети и аксесоари." }
            );
        }
    }
}