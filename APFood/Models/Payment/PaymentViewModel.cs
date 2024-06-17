using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Payment
{
    public class PaymentViewModel
    {
        public required PaymentFormModel PaymentFormModel;
        public required CheckoutCartRequestModel CheckoutCartRequest { get; set; }
        public required List<CartItem> CartItems { get; set; }
        public required OrderSummaryModel OrderSummary { get; set; }
        public List<int> YearRange { get; set; } = Enumerable.Range(DateTime.Now.Year, 10).ToList();
     
    }
}
