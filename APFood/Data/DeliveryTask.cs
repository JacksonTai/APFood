using APFood.Areas.Identity.Data;
using APFood.Constants;

namespace APFood.Data
{
    public class DeliveryTask
    {
        public int Id { get; set; }
        public Customer Runner { get; set; }
        public Order Order { get; set; }
        public DeliveryStatus Status { get; set; }
        public string Location { get; set; }
    }
}
