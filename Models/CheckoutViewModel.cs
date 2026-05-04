using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class CheckoutViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Phone { get; set; }

        public string PaymentMethod { get; set; } = "COD";

        public List<Cart>? CartItems { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
