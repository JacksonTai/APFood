using APFood.Constants.Order;
using APFood.Data;
using APFood.Services.Contract;
using APFood.Models.Cart;
using APFood.Models.Payment;
using APFood.Areas.Identity.Data;
using APFood.Models.Order;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Payment> CreatePayment(Order order, OrderSummaryModel orderSummary)
        {
            try
            {
                // Detach all entities of type CartItem with the same Id
                var trackedCartItems = _context.ChangeTracker.Entries<CartItem>()
                    .Where(e => order.Items.Any(item => item.Id == e.Entity.Id))
                    .ToList();
                foreach (var trackedCartItem in trackedCartItems)
                {
                    _context.Entry(trackedCartItem.Entity).State = EntityState.Detached;
                }

                // Detach all entities of type Food with the same Id
                var trackedFoods = _context.ChangeTracker.Entries<Food>()
                    .Where(e => order.Items.Any(item => item.Food.Id == e.Entity.Id))
                    .ToList();
                foreach (var trackedFood in trackedFoods)
                {
                    _context.Entry(trackedFood.Entity).State = EntityState.Detached;
                }

                // Avoid attaching the order if it is already tracked
                var orderEntry = _context.Entry(order);
                if (orderEntry.State == EntityState.Detached)
                {
                    _context.Attach(order);
                }

                // Ensure related entities are not being tracked multiple times
                foreach (var item in order.Items)
                {
                    var cartItemEntry = _context.Entry(item);
                    if (cartItemEntry.State == EntityState.Detached)
                    {
                        cartItemEntry.State = EntityState.Unchanged;
                    }

                    var foodEntry = _context.Entry(item.Food);
                    if (foodEntry.State == EntityState.Detached)
                    {
                        foodEntry.State = EntityState.Unchanged;
                    }
                }

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
            Cart cart = await _cartService.GetCartAsyncWithTracking(userId) ?? throw new Exception("Cart not found");
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
            Cart cart = await _cartService.GetCartAsyncNoTracking(userId) ?? throw new Exception("Failed to retrieve the cart.");
            CartFormModel cartForm = _sessionManager.Get<CartFormModel>(typeof(CartFormModel).Name) 
                ?? throw new Exception("Cart form not found");

            CreateOrderResponseModel createOrderResponseModel = await _orderService.CreateOrder(cart, cartForm.DineInOption);
            Order createdOrder = await _orderService.GetOrderByIdAsync(createOrderResponseModel.OrderId)
                ?? throw new Exception("Failed to retrieve the created order");
            OrderSummaryModel orderSummary = _orderService.CalculateOrderSummary(cart, cartForm);

            if (cartForm.DineInOption == DineInOption.Delivery && !string.IsNullOrEmpty(cartForm.Location))
            {
                await _deliveryTaskService.CreateDeliveryTask(createdOrder, cartForm.Location);
            }

            Payment createdPayment = await CreatePayment(createdOrder, orderSummary);
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
