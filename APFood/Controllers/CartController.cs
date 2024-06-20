using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    public class CartController(
        ICartService cartService,
        ILogger<CartController> logger
        ) : Controller
    {
        private readonly ICartService _cartService = cartService;
        private readonly ILogger<CartController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                CartViewModel cartView = await _cartService.GetCartViewAsync(GetUserId());
                return View(cartView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the cart for user {UserId}", GetUserId());
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(CartFormModel cartForm)
        {
            string userId = GetUserId();
            try
            {
                CartViewModel cartView = await _cartService.CheckoutCart(userId, cartForm);
                return RedirectToAction("Index", "Payment");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("Location", ex.Message);
                return View("Index", await _cartService.GetCartViewAsync(userId, cartForm));
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
                return Json(await _cartService.UpdateRunnerPointsAsync(cart, isUsingRunnerPoints));
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
                return Json(await _cartService.UpdateDiningOption(cart, dineInOption));
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
