using System.ComponentModel.DataAnnotations;

namespace APFood.Areas.Identity.Data;

// Add profile data for application users by adding properties to the APFoodUser class
public class FoodVendor : APFoodUser
{

    [Required]
    public required string storeName { get; set; }

}

