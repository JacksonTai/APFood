using APFood.Constants;
using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class OrderDetailViewModel
    {
        public required int OrderId { get; set; }
        public required OrderStatus Status { get; set; }
        public required DateTime OrderTime { get; set; }
        public required DineInOption DineInOption { get; set; }
        public required int QueueNumber { get; set; }
        public string? DeliveryLocation { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public string? Runner { get; set; }
        public required List<Data.OrderItem> Items { get; set; }
        public required OrderSummaryModel OrderSummary { get; set; }
        public required bool IsReceivableOrder { get; set; }
        public required bool IsCancellableOrder { get; set; }
    }
}
