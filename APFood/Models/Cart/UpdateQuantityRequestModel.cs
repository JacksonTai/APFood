namespace APFood.Models.Cart
{
    public class UpdateQuantityRequestModel
    {
        public int ItemId { get; set; }
        public int NewQuantity { get; set; }
    }
}
