using APFood.Areas.Identity.Data;
using APFood.Constants.Order;

namespace APFood.Data
{
    public class Order
    {
        public int Id { get; set; }
        public required string CustomerId { get; set; }
        public required Customer Customer { get; set; }
        public Payment? Payment { get; set; }
        public required List<OrderItem> Items { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DineInOption DineInOption { get; set; } = DineInOption.Pickup;
        public string? QueueNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
