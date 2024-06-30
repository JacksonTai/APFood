using APFood.Constants;
using APFood.Constants.Order;

namespace APFood.Models.DeliveryTask
{
    public class DeliveryTaskListViewModel
    {
        public required int DeliveryTaskId { get; set; }
        public required string DeliveryLocation { get; set; }
        public required DeliveryStatus DeliveryStatus { get; set; }
        public required int QueueNumber { get; set; }
        public required string CustomerName { get; set; }
        public required int OrderId { get; set; }
        public required OrderStatus OrderStatus { get; set; }
        public required DateTime OrderTime { get; set; }
        public required bool IsAcceptableDeliveryTask { get; set; }
        public required bool IsDeliverableDeliveryTask { get; set; }
        public required bool IsCancellableDeliveryTask { get; set; }
    }
}
