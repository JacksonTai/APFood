using APFood.Areas.Identity.Data;

namespace APFood.Data
{
    public class FoodVendor : APFoodUser
    {
        public string? StoreName { get; set; }
    }
}
