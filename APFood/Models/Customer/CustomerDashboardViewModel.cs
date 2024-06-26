using System.Collections.Generic;
using APFood.Data;

namespace APFood.Models.Customer
{
    public class CustomerDashboardViewModel
    {
        public List<Data.FoodVendor>? FoodVendors { get; set; }
        public List<Food>? FoodItems { get; set; }
        public string? SelectedVendorId { get; set; }
        public Dictionary<int, int>? CartItems { get; set; }
    }
}
