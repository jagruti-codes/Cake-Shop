using System;
using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string Phone { get; set; }

        public DateTime? EventDate { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
