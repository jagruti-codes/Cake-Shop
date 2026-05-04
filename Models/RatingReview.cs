namespace cake_shop.Models
{
    public class RatingReview
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public AppUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Rating { get; set; } // 1 to 5

        public string? Review { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public bool IsApproved { get; set; } = false;

    }
}
