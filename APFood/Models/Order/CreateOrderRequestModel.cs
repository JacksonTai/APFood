using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class CreateOrderRequestModel
    {
        public int CartId { get; set; }
        public DineInOption DineInOption { get; set; }
    }
}
