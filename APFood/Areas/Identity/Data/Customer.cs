using APFood.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APFood.Areas.Identity.Data
{
    public class Customer : APFoodUser
    {
        [Required]
        public required string FullName { get; set; }

        [ForeignKey("Cart")]
        public int? CartId { get; set; }
        public Cart? Cart { get; set; }
    }
}
