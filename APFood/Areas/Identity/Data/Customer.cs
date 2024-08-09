using APFood.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace APFood.Areas.Identity.Data
{
    public class Customer : APFoodUser
    {
        [Required]
        public required string FullName { get; set; }

        [ForeignKey("Cart")]
        public int? CartId { get; set; }
 
        [JsonIgnore]
        public Cart? Cart { get; set; }

        [DefaultValue(0.0)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Points { get; set; }
    }
}
