using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Admin
{
    public class FoodVendorViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        public string StoreName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
