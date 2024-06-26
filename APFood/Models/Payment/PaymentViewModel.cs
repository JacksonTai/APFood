using APFood.Data;
using APFood.Models.Order;

namespace APFood.Models.Payment
{
    public class PaymentViewModel
    {
        public List<int> YearRange { get; set; } = Enumerable.Range(DateTime.Now.Year, 10).ToList();
        public required PaymentFormModel PaymentForm;
        public required List<CartItem> CartItems { get; set; }
        public required OrderSummaryModel OrderSummary { get; set; }
        public required bool IsUsingRunnerPoints { get; set; }
    }
}
