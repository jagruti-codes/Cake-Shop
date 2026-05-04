
using System.Collections.Generic;

namespace cake_shop.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCategories { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<Order> RecentOrders { get; set; }

        public List<TopProductVM> TopProducts { get; set; }
    }

    public class TopProductVM
    {
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public int TotalSold { get; set; }
        public decimal Price { get; set; }
    }
}
