using APFood.Constants.Order;
using APFood.Constants;
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

        public async Task<Payment> CreatePayment(Order order, bool IsUsingRunnerPoints)
        {
            try
            {
                decimal subtotal = order.Items.Sum(item => item.Food.Price * item.Quantity);
                decimal deliveryFee = order.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
                decimal redeemedRunnerPoints = IsUsingRunnerPoints ? order.Customer.Points : 0;
                decimal total = subtotal + deliveryFee - redeemedRunnerPoints;

                Payment payment = new()
                {
                    OrderId = order.Id,
                    Order = order,
                    Subtotal = subtotal,
                    DeliveryFee = deliveryFee,
                    RunnerPointsUsed = redeemedRunnerPoints,
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

        public async Task<PaymentViewModel> GetPaymentViewAsync(string userId, PaymentFormModel paymentForm)
        {
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) 
                ?? throw new Exception("Cart form not found");

            List<CartItem> cartItems = await _cartService.GetCartItemsAsync(userId);
            Customer customer = await _context.Customers.FindAsync(userId) ?? throw new Exception("User not found");

            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal subtotal = await _cartService.GetCartTotalAsync(userId);
            decimal runnerPointsRedeemed = cartForm.IsUsingRunnerPoints ? customer.Points : 0;
            decimal total = subtotal + deliveryFee - runnerPointsRedeemed;

            return new PaymentViewModel
            {
                CartItems = cartItems,
                OrderSummary = new OrderSummaryModel
                {
                    Subtotal = subtotal,
                    DeliveryFee = deliveryFee,
                    RunnerPointsRedeemed = runnerPointsRedeemed,
                    Total = total
                },
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
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) 
                ?? throw new Exception("Cart form not found");

            DineInOption dineInOption = cartForm.DineInOption;
            string? location = cartForm.Location;

            Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Failed to retrieve the cart.");
            Order createdOrder = await _orderService.CreateOrder(cart, dineInOption);
            
            if (dineInOption == DineInOption.Delivery && !string.IsNullOrEmpty(location))
            {
                await _deliveryTaskService.CreateDeliveryTask(createdOrder, location);
            }

            await CreatePayment(createdOrder, cartForm.IsUsingRunnerPoints);
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
