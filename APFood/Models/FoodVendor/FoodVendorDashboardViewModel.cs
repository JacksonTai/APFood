using APFood.Constants.Order;
using APFood.Models.FoodVendor;
using System.Collections.Generic;

namespace APFood.Models
{
    public class FoodVendorDashboardViewModel
    {
        public string StoreName { get; set; }
        public int ProcessingCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal TotalProfit { get; set; }
        public List<VendorOrderViewModel> ProcessingOrders { get; set; }
        public List<VendorOrderViewModel> CompletedOrders { get; set; }
        public List<VendorOrderViewModel> CancelledOrders { get; set; }
        public Dictionary<OrderStatus, int> OrderCounts { get; set; }
    }
}
