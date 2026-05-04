
using Microsoft.EntityFrameworkCore;
using cake_shop.Models;
namespace cake_shop.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        
        public DbSet<Category> Categories { get; set; } 
        public DbSet<Product> Products { get; set; }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Shipping> Shippings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RatingReview> RatingReviews { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }

    }
}
