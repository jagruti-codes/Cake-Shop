


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace cake_shop.Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }

        public string Status { get; set; } = "Pending";

        public string PaymentMethod { get; set; } = "COD";
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
