using APFood.Areas.Identity.Data;
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
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    public class PaymentController(
        IPaymentService paymentService,
        ILogger<PaymentController> logger) : Controller
    {
        private readonly IPaymentService _paymentService = paymentService;
        private readonly ILogger<PaymentController> _logger = logger;

        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            try
            {
                PaymentViewModel paymentView = await _paymentService.GetPaymentViewAsync(userId);
                return View(paymentView);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "An error occurred while loading the payment page for user {UserId}", userId);
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(PaymentFormModel paymentForm)
        {
            string userId = GetUserId();
            try
            {
                return RedirectToAction("Success", "Payment",
                    await _paymentService.ProcessPaymentAsync(userId, paymentForm));
            }
            catch (ArgumentException)
            {
                return View("Index", await _paymentService.GetPaymentViewAsync(userId, paymentForm));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the payment for user {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Payment processing failed. Please try again.");
                return View("Index", await _paymentService.GetPaymentViewAsync(userId, paymentForm));
            }
        }

        public IActionResult Success(PaymentSuccessViewModel paymentSuccessView)
        {
            if (paymentSuccessView.OrderId.HasValue && paymentSuccessView.OrderId.HasValue)
            {
                return View(paymentSuccessView);
            }
            return RedirectToAction("Index", "Home");
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated.");
        }
    }
}
