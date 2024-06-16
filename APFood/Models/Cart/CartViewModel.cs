using APFood.Data;
using APFood.Models.Order;

namespace APFood.Models.Cart
{
    public class CartViewModel
    {
        public required List<CartItem> CartItems { get; set; }
        public required OrderSummaryModel OrderSummary { get; set; }
        public required CheckoutCartRequest CheckoutCartRequest { get; set; }
    }
}
