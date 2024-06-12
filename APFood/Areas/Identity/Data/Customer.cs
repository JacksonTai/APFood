using System.ComponentModel.DataAnnotations;

namespace APFood.Areas.Identity.Data
{
    public class Customer : APFoodUser
    {
        [Required]
        public required string FullName { get; set; }
    }
}
