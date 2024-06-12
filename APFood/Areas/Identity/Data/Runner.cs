using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace APFood.Areas.Identity.Data
{
    public class Runner : APFoodUser
    {

        [DefaultValue(0.0)]
        public double? Points { get; set; }

    }
}
