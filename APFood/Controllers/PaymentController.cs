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
        ICartService cartService) : Controller
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly IOrderService _orderService = orderService;
        private readonly ICartService _cartService = cartService;

        public async Task<IActionResult> Index()
        {
            var paymentViewModel = await CreatePaymentViewModelAsync();
            return View(paymentViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PaymentFormModel paymentFormModel)
        {
            switch (paymentFormModel.PaymentMethod)
            {
                case PaymentMethod.CreditCard:
                    return await ProcessCreditCardPayment(paymentFormModel);
                case PaymentMethod.Paypal:
                    return await ProcessPayPalPayment(paymentFormModel);
                default:
                    ModelState.AddModelError(string.Empty, "Invalid payment method");
                    var invalidPaymentViewModel = await CreatePaymentViewModelAsync();
                    invalidPaymentViewModel.PaymentFormModel = paymentFormModel;
                    return View(invalidPaymentViewModel);
            }
        }

        public IActionResult Success()
        {
            return View();
        }

        private async Task<IActionResult> ProcessPayPalPayment(PaymentFormModel paymentFormModel)
        {
            bool paymentProcessed = true; // Mimic the result of PayPal API to process the payment
            if (paymentProcessed)
            {
                Order order = await ProcessPayment();
                return View("Success", order);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Payment processing failed. Please try again.");
                var paymentViewModel = await CreatePaymentViewModelAsync();
                paymentViewModel.PaymentFormModel = paymentFormModel;
                return View("Index", paymentViewModel);
            }
        }

        private async Task<IActionResult> ProcessCreditCardPayment(PaymentFormModel paymentFormModel)
        {
            if (!ModelState.IsValid)
            {
                var paymentViewModel = await CreatePaymentViewModelAsync();
                paymentViewModel.PaymentFormModel = paymentFormModel;
                return View("Index", paymentViewModel);
            }

            bool paymentProcessed = true; // Mimic the result of credit card payment gateway processing
            if (paymentProcessed)
            {
                Order order = await ProcessPayment();
                return View("Success", order);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Payment processing failed. Please try again.");
                var paymentViewModel = await CreatePaymentViewModelAsync();
                paymentViewModel.PaymentFormModel = paymentFormModel;
                return View("Index", paymentViewModel);
            }
        }

        private async Task<Order> ProcessPayment()
        {
            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not found");
            Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception();
            string checkoutCartRequestJson = HttpContext.Session.GetString(typeof(CheckoutCartRequest).Name) ?? throw new Exception();
            CheckoutCartRequest checkoutCartRequest = JsonConvert.DeserializeObject<CheckoutCartRequest>(checkoutCartRequestJson) ?? throw new Exception();

            Order createdOrder = await _orderService.CreateOrder(cart, checkoutCartRequest.DineInOption);
            await _paymentService.CreatePayment(createdOrder);

            await _cartService.ClearCartAsync(userId);
            HttpContext.Session.Remove(typeof(CheckoutCartRequest).Name);
            return createdOrder;
        }

        private async Task<PaymentViewModel> CreatePaymentViewModelAsync()
        {
            string checkoutCartRequestJson = HttpContext.Session.GetString(typeof(CheckoutCartRequest).Name) ?? throw new Exception();
            CheckoutCartRequest checkoutCartRequest = JsonConvert.DeserializeObject<CheckoutCartRequest>(checkoutCartRequestJson) ?? throw new Exception();

            string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("User not found");
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
                    Total = isDelivery ? total += OrderConstants.DeliveryFee : total
                },
                PaymentFormModel = new PaymentFormModel(),
            };
        }
    }
}
