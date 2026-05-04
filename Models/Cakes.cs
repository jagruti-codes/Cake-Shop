using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class Cakes
    {
        [Key]
        public int CakeId { get; set; }

        [Required(ErrorMessage = "Cake name is required")]
        [StringLength(100)]
        public string CakeName { get; set; }

        [Required]
        public string Category { get; set; }

        [Required]
        [Range(1, 10000)]
        public decimal Price { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
