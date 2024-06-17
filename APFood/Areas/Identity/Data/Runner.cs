using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APFood.Areas.Identity.Data
{
    public class Runner : APFoodUser
    {

        [DefaultValue(0.0)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Points { get; set; }

    }
}
