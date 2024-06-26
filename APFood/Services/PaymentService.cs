using APFood.Constants.Order;
using APFood.Data;
using APFood.Services.Contract;
using APFood.Models.Cart;
using APFood.Models.Payment;
using APFood.Areas.Identity.Data;
using APFood.Models.Order;

namespace APFood.Services
{
    public class PaymentService(
        APFoodContext context,
        SessionManager sessionManager,
        ICartService cartService,
        IOrderService orderService,
        IDeliveryTaskService deliveryTaskService,
        ILogger<PaymentService> logger
        ) : IPaymentService
    {
        private readonly APFoodContext _context = context;
        private readonly SessionManager _sessionManager = sessionManager;
        private readonly ICartService _cartService = cartService;
        private readonly IOrderService _orderService = orderService;
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;
        private readonly ILogger<PaymentService> _logger = logger;

        public async Task<Payment> CreatePayment(Order order, CartFormModel cartForm)
        {
            try
            {
                Cart cart = await _cartService.GetCartAsync(order.CustomerId) ?? throw new Exception("Cart not found");
                OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);
                Payment payment = new()
                {
                    OrderId = order.Id,
                    Order = order,
                    Subtotal = orderSummary.Subtotal,
                    DeliveryFee = orderSummary.DeliveryFee,
                    RunnerPointsUsed = orderSummary.RunnerPointsRedeemed,
                    Total = orderSummary.Total,
                };
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

        public async Task<PaymentViewModel> GetPaymentViewAsync(string userId, PaymentFormModel paymentForm)
        {
            Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) 
                ?? throw new Exception("Cart form not found");
 
            return new PaymentViewModel
            {
                CartItems = await _cartService.GetCartItemsAsync(userId),
                OrderSummary = _orderService.CalculateOrderSummary(cart, cartForm),
                PaymentForm = paymentForm,
                IsUsingRunnerPoints = cartForm.IsUsingRunnerPoints
            };
        }

        public async Task<PaymentViewModel> GetPaymentViewAsync(string userId)
        {
            return await GetPaymentViewAsync(userId, new PaymentFormModel());
        }

        public async Task<PaymentSuccessViewModel> ProcessPaymentAsync(string userId, PaymentFormModel paymentFormModel)
        {
            Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Failed to retrieve the cart.");
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) 
                ?? throw new Exception("Cart form not found");

            Order createdOrder = await _orderService.CreateOrder(cart, cartForm.DineInOption);
            if (cartForm.DineInOption == DineInOption.Delivery && !string.IsNullOrEmpty(cartForm.Location))
            {
                await _deliveryTaskService.CreateDeliveryTask(createdOrder, cartForm.Location);
            }
            Payment createdPayment = await CreatePayment(createdOrder, cartForm);
            Customer customer = await _context.Customers.FindAsync(userId) ?? throw new Exception("User not found");
            customer.Points -= createdPayment.RunnerPointsUsed;
            await _context.SaveChangesAsync();
            await _cartService.ClearCartAsync(userId);
            _sessionManager.Remove(typeof(CartFormModel).Name);

            return new PaymentSuccessViewModel
            {
                OrderId = createdOrder.Id,
                QueueNumber = createdOrder.QueueNumber
            };
        }
    }
}
