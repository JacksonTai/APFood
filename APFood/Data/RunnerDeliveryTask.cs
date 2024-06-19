using APFood.Areas.Identity.Data;
using APFood.Constants;

namespace APFood.Data
{
    public class RunnerDeliveryTask
    {
        public required string RunnerId { get; set; }
        public int DeliveryTaskId { get; set; }
        public required DeliveryStatus Status { get; set; } = DeliveryStatus.Accepted;

        public required Customer Runner { get; set; }
        public required DeliveryTask DeliveryTask { get; set; }
    }
}
