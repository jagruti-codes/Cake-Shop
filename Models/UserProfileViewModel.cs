using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter valid email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number required")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Enter valid phone number")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(120)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        public bool ShowEditForm { get; set; }
    }
}
