using System.ComponentModel.DataAnnotations;

namespace cake_shop.Models
{
    public class Category
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public int ItemCount { get; set; } = 0;
    }
}
