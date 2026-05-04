namespace cake_shop.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public string? WeekdayStart { get; set; }
        public string? WeekdayEnd { get; set; }

        public string? WeekendStart { get; set; }
        public string? WeekendEnd { get; set; }

        public bool ApplyHolidayHours { get; set; }
    }
}
