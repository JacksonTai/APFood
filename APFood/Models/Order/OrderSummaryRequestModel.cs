using APFood.Constants.Order;

namespace APFood.Models.Order
{
    public class OrderSummaryRequestModel
    {
        public Data.Cart Cart { get; set; }
        public DineInOption DineInOption { get; set; }
        public bool IsUsingRunnerPoints { get; set; }
    }
}
