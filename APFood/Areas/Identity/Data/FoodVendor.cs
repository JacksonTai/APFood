using System.ComponentModel.DataAnnotations;

namespace APFood.Areas.Identity.Data;

public class FoodVendor : APFoodUser
{

    [Required]
    public required string StoreName { get; set; }

}

