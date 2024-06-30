using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class OrderViewModel
    {
        public required List<OrderListViewModel> OrderList { get; set; }
        public required Dictionary<OrderStatus, int> OrderCounts { get; set; }
        public required OrderStatus CurrentStatus { get; set; }
        public required decimal TotalPointsSpent { get; set; }
        public bool IsAdmin { get; set; }
    }
}
