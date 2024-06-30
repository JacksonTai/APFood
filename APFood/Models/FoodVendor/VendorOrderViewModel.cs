namespace APFood.Models.FoodVendor
{
    public class VendorOrderViewModel
    {
        public string OrderId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string FoodName { get; set; }
        public int Quantity { get; set; }
        public string DateTime { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public APFood.Constants.Order.DineInOption DineInOption { get; set; }
    }
}
