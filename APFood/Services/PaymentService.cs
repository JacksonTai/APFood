using APFood.Constants.Order;
using APFood.Constants;
using APFood.Data;
using APFood.Services.Contract;

namespace APFood.Services
{
    public class PaymentService(APFoodContext context, ILogger<PaymentService> logger) : IPaymentService
    {
        private readonly APFoodContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly ILogger<PaymentService> _logger = logger;

        public async Task<Payment> CreatePayment(Order order)
        {
            try
            {
                bool isDelivery = order.DineInOption == DineInOption.Delivery;
                decimal subtotal = order.Items.Sum(item => item.Food.Price * item.Quantity);
                decimal deliveryFee = isDelivery ? OrderConstants.DeliveryFee : 0;
                decimal total = subtotal + deliveryFee;

                Payment payment = new()
                {
                    OrderId = order.Id,
                    Order = order,
                    Subtotal = subtotal,
                    DeliveryFee = deliveryFee,
                    Total = total,
                };

                ArgumentNullException.ThrowIfNull(payment);
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the payment record");
                throw;
            }
        }
    }
}
