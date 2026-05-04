using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cake_shop.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
