using APFood.Data;

namespace APFood.Services.Contract
{
    public interface IPaymentService
    {
        Task<Payment> CreatePayment(Order order);

    }
}
