using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace cake_shop.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; } = "Cash";
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
