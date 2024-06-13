namespace APFood.Models.Cart
{
    public class UpdateCartItemQuantityRequest
    {
        public int ItemId { get; set; }
        public int NewQuantity { get; set; }
    }
}
