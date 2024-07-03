using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Services;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    public class CartController(
        ICartService cartService,
        ILogger<CartController> logger,
        S3Service s3Service
        ) : Controller
    {
        private readonly ICartService _cartService = cartService;
        private readonly ILogger<CartController> _logger = logger;
        private readonly S3Service _s3Service = s3Service;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string userId = GetUserId();
            try
            {
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
                foreach (var item in cart.Items)
                {
                    if (!string.IsNullOrEmpty(item.Food.ImageUrl))
                    {
                        var fileName = Path.GetFileName(item.Food.ImageUrl);
                        item.Food.ImageUrl = _s3Service.GeneratePreSignedURL(fileName, TimeSpan.FromMinutes(30));
                    }
                }
                CartViewModel cartView = await _cartService.GetCartViewAsync(cart);
                return View(cartView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the cart for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(CartFormModel cartForm)
        {
            string userId = GetUserId();
            Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
            try
            {
                CartViewModel cartView = await _cartService.CheckoutCart(cart, cartForm);
                return RedirectToAction("Index", "Payment");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Location", ex.Message);

                return View("Index", await _cartService.GetCartViewAsync(cart, cartForm));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the checkout for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        public async Task<IActionResult> AddItemAsync(Food food, int quantity)
        {
            string userId = GetUserId();    
            try
            {
                await _cartService.AddItemAsync(userId, food, quantity);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding item to cart for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateQuantityRequestModel updateQuantityRequest)
        {
            string userId = GetUserId();
            try
            {
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
                return Json(await _cartService.UpdateQuantityAsync(cart, updateQuantityRequest));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating item quantity for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            string userId = GetUserId();
            try
            {
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
                return Json(await _cartService.RemoveItemAsync(cart, itemId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing item from cart for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRunnerPoints(bool isUsingRunnerPoints)
        {
            string userId = GetUserId();
            try
            {
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
                return Json(_cartService.UpdateRunnerPoints(cart, isUsingRunnerPoints));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating runner points for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdateDineInOption(DineInOption dineInOption)
        {
            string userId = GetUserId();
            try
            {
                Cart cart = await _cartService.GetCartAsync(userId) ?? throw new Exception("Cart not found");
                return Json(_cartService.UpdateDiningOption(cart, dineInOption));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating dine-in option for user {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated");
        }

    }
}
