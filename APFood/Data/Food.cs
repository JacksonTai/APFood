using System.ComponentModel.DataAnnotations.Schema;

namespace APFood.Data
{
    public class Food
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


    }
}
