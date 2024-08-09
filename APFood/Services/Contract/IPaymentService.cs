using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Models.Payment;

namespace APFood.Services.Contract
{
    public interface IPaymentService
    {
        Task<Payment> CreatePayment(Order order, OrderSummaryModel orderSummary);
        Task<PaymentViewModel> GetPaymentViewAsync(string userId, PaymentFormModel paymentForm);
        Task<PaymentViewModel> GetPaymentViewAsync(string userId);
        Task<PaymentSuccessViewModel> ProcessPaymentAsync(string userId, PaymentFormModel paymentFormModel);
    }
}
