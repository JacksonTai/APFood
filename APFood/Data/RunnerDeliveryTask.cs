using APFood.Areas.Identity.Data;

namespace APFood.Data
{
    public class RunnerDeliveryTask
    {
        public int Id { get; set; }
        public required string RunnerId { get; set; }
        public required Customer Runner { get; set; }
        public int DeliveryTaskId { get; set; }
        public required DeliveryTask DeliveryTask { get; set; }

    }
}
