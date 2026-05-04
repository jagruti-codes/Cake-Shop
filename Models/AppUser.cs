

using System;
using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        public bool IsEmailVerified { get; set; } = false;

        public string? OTP { get; set; }
        public DateTime? OTPExpiry { get; set; }
    }
}
