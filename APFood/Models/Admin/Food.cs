using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Admin
{
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public string FoodVendorId { get; set; }
        public APFood.Areas.Identity.Data.FoodVendor FoodVendor { get; set; }
        public string Status { get; set; }
    }
}
