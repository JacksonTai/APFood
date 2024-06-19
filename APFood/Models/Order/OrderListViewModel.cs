using APFood.Constants;
using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class OrderListViewModel
    {
        public required int OrderId { get; set; }
        public required int QueueNumber { get; set; }
        public required DateTime OrderTime { get; set; }
        public required DineInOption DineInOption { get; set; }
        public required decimal TotalPrice { get; set; }
        public required OrderStatus OrderStatus { get; set; }
        public required bool CanShowReceivedButton { get; set; }
    }
}
