using APFood.Constants;

namespace APFood.Data
{
    public class DeliveryTask
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public required string Location { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public List<RunnerDeliveryTask> RunnerDeliveryTasks { get; set; } = [];
        public required Order Order { get; set; }
    }
}
