using APFood.Constants.Order;
using System.Collections.Generic;

namespace APFood.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public int ProcessingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<AdminOrderViewModel> RecentOrders { get; set; }
        public Dictionary<OrderStatus, int> OrderCounts { get; set; }
    }

    public class AdminOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string FoodItems { get; set; }
        public string DateTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
    }
}
