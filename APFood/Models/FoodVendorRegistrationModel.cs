using System.ComponentModel.DataAnnotations;

namespace APFood.Models
{
    public class FoodVendorRegistrationModel : RegistrationModel
    {
        [Required]
        [Display(Name = "Store Name")]

        public required string storeName { get; set; }
    }
    
}
