using APFood.Constants;
using APFood.Constants.Order;
using APFood.Constants.Payment;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Models.Payment;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = "Customer")]
    public class PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ICartService cartService,
        IDeliveryTaskService deliveryTaskService,
        ILogger<PaymentController> logger) : Controller
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IOrderService _orderService = orderService;
        private readonly ICartService _cartService = cartService;
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;
        private readonly ILogger<PaymentController> _logger = logger;

        public async Task<IActionResult> Index()
        {
            try
            {
                PaymentViewModel paymentViewModel = await CreatePaymentViewModelAsync();
                return View(paymentViewModel);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Checkout cart request is missing in the session.");
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(PaymentFormModel paymentFormModel)
        {
            return paymentFormModel.PaymentMethod switch
            {
                PaymentMethod.CreditCard => await HandlePaymentProcessing(ProcessCreditCardPayment, paymentFormModel),
                PaymentMethod.Paypal => await HandlePaymentProcessing(ProcessPayPalPayment, paymentFormModel),
                _ => await ReturnWithErrorAsync(paymentFormModel)
            };
        }

        public IActionResult Success(PaymentSuccessViewModel paymentSuccessViewModel)
        {
            if (paymentSuccessViewModel.OrderId.HasValue && paymentSuccessViewModel.OrderId.HasValue)
            {
                return View(paymentSuccessViewModel);
            }
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> HandlePaymentProcessing(
             Func<Task<bool>> paymentProcessingFunc, PaymentFormModel paymentFormModel)
        {
            try
            {
                bool paymentProcessed = await paymentProcessingFunc();
                if (paymentProcessed)
                {
                    Order order = await ProcessPayment();
                    PaymentSuccessViewModel paymentSuccessViewModel = new()
                    {
                        OrderId = order.Id,
                        QueueNumber = order.QueueNumber
                    };
                    return RedirectToAction("Success", "Payment", paymentSuccessViewModel);
                }
                else
                {
                    return await ReturnWithErrorAsync(paymentFormModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during payment processing.");
                return await ReturnWithErrorAsync(paymentFormModel);
            }
        }

        private async Task<bool> ProcessPayPalPayment()
        {
            // Simulate PayPal processing
            await Task.Delay(1000);
            return true;
        }

        private async Task<bool> ProcessCreditCardPayment()
        {
            if (!ModelState.IsValid)
            {
                return false;
            }
            // Simulate credit card payment processing
            await Task.Delay(1000);
            return true;
        }

        private async Task<Order> ProcessPayment()
        {
            try
            {
                string checkoutCartRequestJson = GetCheckoutCartRequestFromSession();
                string userId = GetUserId();
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Failed to retrieve the cart.");

                CheckoutCartRequestModel checkoutCartRequest =
                    JsonConvert.DeserializeObject<CheckoutCartRequestModel>(checkoutCartRequestJson)
                    ?? throw new Exception("Failed to deserialize checkout cart request.");

                DineInOption dineInOption = checkoutCartRequest.DineInOption;
                string? location = checkoutCartRequest.Location;

                Order createdOrder = await _orderService.CreateOrder(cart, dineInOption);
                if (dineInOption == DineInOption.Delivery && !string.IsNullOrEmpty(location))
                {
                    await _deliveryTaskService.CreateDeliveryTask(createdOrder, location);
                }
                await _paymentService.CreatePayment(createdOrder);
                await _cartService.ClearCartAsync(userId);
                HttpContext.Session.Remove(typeof(CheckoutCartRequestModel).Name);


                return createdOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the payment.");
                throw new InvalidOperationException("An error occurred while processing the payment.", ex);
            }
        }

        private async Task<PaymentViewModel> CreatePaymentViewModelAsync()
        {
            try
            {
                string checkoutCartRequestJson = GetCheckoutCartRequestFromSession();
                CheckoutCartRequestModel checkoutCartRequest =
                    JsonConvert.DeserializeObject<CheckoutCartRequestModel>(checkoutCartRequestJson)
                    ?? throw new InvalidOperationException("Failed to deserialize checkout cart request.");

                string userId = GetUserId();
                List<CartItem> cartItems = await _cartService.GetCartItemsAsync(userId);

                decimal subtotal = await _cartService.GetTotalAsync(userId);
                decimal total = subtotal;
                bool isDelivery = checkoutCartRequest.DineInOption == DineInOption.Delivery;

                return new PaymentViewModel
                {
                    CartItems = cartItems,
                    CheckoutCartRequest = checkoutCartRequest,
                    OrderSummary = new OrderSummaryModel
                    {
                        Subtotal = subtotal,
                        DeliveryFee = isDelivery ? OrderConstants.DeliveryFee : 0,
                        Total = isDelivery ? total + OrderConstants.DeliveryFee : total
                    },
                    PaymentFormModel = new PaymentFormModel(),
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the payment view model.");
                throw new InvalidOperationException("An error occurred while creating the payment view model.", ex);
            }
        }

        private async Task<IActionResult> ReturnWithErrorAsync(PaymentFormModel paymentFormModel)
        {
            ModelState.AddModelError(string.Empty, "Payment processing failed. Please try again.");
            PaymentViewModel paymentViewModel = await CreatePaymentViewModelAsync();
            paymentViewModel.PaymentFormModel = paymentFormModel;
            return View("Index", paymentViewModel);
        }

        private string GetCheckoutCartRequestFromSession()
        {
            string? checkoutCartRequestJson = HttpContext.Session.GetString(typeof(CheckoutCartRequestModel).Name);
            if (string.IsNullOrEmpty(checkoutCartRequestJson))
            {
                throw new InvalidOperationException("No checkout cart request found in session.");
            }
            return checkoutCartRequestJson;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? throw new UnauthorizedAccessException("User not authenticated.");
        }
    }
}
