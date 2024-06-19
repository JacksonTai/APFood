using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    public class CartController(ICartService cartService, ILogger<CartController> logger) : Controller
    {
        private readonly ICartService _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
        private readonly ILogger<CartController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                var checkoutCartRequest = GetCheckoutCartRequestFromSession() ?? new CheckoutCartRequestModel();
                var cartViewModel = await CreateCartViewModel(userId, checkoutCartRequest);

                return View(cartViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the cart for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutCartRequestModel checkoutCartRequest)
        {
            try
            {
                var userId = GetUserId();
                var cartViewModel = await CreateCartViewModel(userId, checkoutCartRequest);
                bool isDelivery = checkoutCartRequest.DineInOption == DineInOption.Delivery;

                if (isDelivery && !ModelState.IsValid)
                {
                    ModelState.AddModelError("Location", "Location is required for delivery");
                    return View("Index", cartViewModel);
                }

                SetCheckoutCartRequestInSession(checkoutCartRequest);
                return RedirectToAction("Index", "Payment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the checkout for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        public async Task<IActionResult> AddItemAsync(Food food, int quantity)
        {
            try
            {
                var userId = GetUserId();
                await _cartService.AddItemAsync(userId, food, quantity);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding item to cart for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemQuantityModel request)
        {
            try
            {
                await _cartService.UpdateQuantityAsync(request.ItemId, request.NewQuantity);
                string userId = GetUserId();
                Cart? cart = await _cartService.GetCartAsync(userId);
                CartItem? cartItem = cart?.Items.FirstOrDefault(ci => ci.Id == request.ItemId);
                CheckoutCartRequestModel checkoutCartRequest = GetCheckoutCartRequestFromSession() ?? new CheckoutCartRequestModel();
                bool isDelivery = checkoutCartRequest.DineInOption == DineInOption.Delivery;
                decimal subtotal = await _cartService.GetTotalAsync(userId);
                decimal total = isDelivery ? subtotal + OrderConstants.DeliveryFee : subtotal;

                return Json(new { itemPrice = cartItem?.Food.Price * cartItem?.Quantity, subtotal, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating item quantity for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            try
            {
                string userId = GetUserId();
                await _cartService.RemoveItemAsync(itemId);
                CheckoutCartRequestModel checkoutCartRequest = GetCheckoutCartRequestFromSession() ?? new CheckoutCartRequestModel();
                bool isDelivery = checkoutCartRequest.DineInOption == DineInOption.Delivery;
                decimal subtotal = await _cartService.GetTotalAsync(userId);
                decimal total = isDelivery ? subtotal + OrderConstants.DeliveryFee : subtotal;

                return Json(new { subtotal, total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing item from cart for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDineInOption(string dineInOption)
        {
            try
            {
                var userId = GetUserId();
                var checkoutCartRequest = GetCheckoutCartRequestFromSession() ?? new CheckoutCartRequestModel();
                var subtotal = await _cartService.GetTotalAsync(userId);

                if (Enum.TryParse(dineInOption, out DineInOption option))
                {
                    checkoutCartRequest.DineInOption = option;
                    SetCheckoutCartRequestInSession(checkoutCartRequest);

                    var deliveryFee = option == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
                    var total = subtotal + deliveryFee;

                    return Json(new { deliveryFee, total });
                }

                return BadRequest("Invalid dine-in option");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating dine-in option for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not found");
        }

        private CheckoutCartRequestModel? GetCheckoutCartRequestFromSession()
        {
            var checkoutCartRequestJson = HttpContext.Session.GetString(typeof(CheckoutCartRequestModel).Name);
            return !string.IsNullOrEmpty(checkoutCartRequestJson)
                ? JsonConvert.DeserializeObject<CheckoutCartRequestModel>(checkoutCartRequestJson)
                : null;
        }

        private void SetCheckoutCartRequestInSession(CheckoutCartRequestModel checkoutCartRequest)
        {
            HttpContext.Session.SetString(typeof(CheckoutCartRequestModel).Name, JsonConvert.SerializeObject(checkoutCartRequest));
        }

        private async Task<CartViewModel> CreateCartViewModel(string userId, CheckoutCartRequestModel checkoutCartRequest)
        {
            var subtotal = await _cartService.GetTotalAsync(userId);
            var isDelivery = checkoutCartRequest.DineInOption == DineInOption.Delivery;

            return new CartViewModel
            {
                CartItems = await _cartService.GetCartItemsAsync(userId),
                CheckoutCartRequest = checkoutCartRequest,
                OrderSummary = new OrderSummaryModel
                {
                    Subtotal = subtotal,
                    DeliveryFee = isDelivery ? OrderConstants.DeliveryFee : 0,
                    Total = isDelivery ? subtotal + OrderConstants.DeliveryFee : subtotal
                }
            };
        }

    }
}
