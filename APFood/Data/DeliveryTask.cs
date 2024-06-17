using APFood.Areas.Identity.Data;
using APFood.Constants;

namespace APFood.Data
{
    public class DeliveryTask
    {
        public int Id { get; set; }
        public required int OrderId { get; set; }
        public required Order Order { get; set; }
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
        public required string Location { get; set; }
    }
}
