using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class OrderListViewModel
    {
        string OrderId { get; set; }
        string QueueNumber { get; set; }
        DateTime OrderTime { get; set; }
        DineInOption DineInOption { get; set; }
    }
}
