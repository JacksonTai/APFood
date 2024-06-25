using System.Collections.Generic;
using APFood.Data;
using APFood.Areas.Identity.Data;

namespace APFood.Models.Customer
{
    public class CustomerDashboardViewModel
    {
        public List<APFood.Areas.Identity.Data.FoodVendor>? FoodVendors { get; set; }
        public List<Food>? FoodItems { get; set; }
        public string? SelectedVendorId { get; set; }
        public Dictionary<int, int>? CartItems { get; set; }
    }
}
