using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Constants.Food;
using APFood.Data;
using APFood.Models.Customer;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.Customer}")]
    public class CustomerController : Controller
    {
        private readonly APFoodContext _context;
        private readonly UserManager<APFoodUser> _userManager;
        private readonly IRunnerPointService _runnerPointService;

        public CustomerController(
            APFoodContext context, 
            UserManager<APFoodUser> userManager,
            IRunnerPointService runnerPointService
            )
        {
            _context = context;
            _userManager = userManager;
            _runnerPointService = runnerPointService;
        }

        public async Task<IActionResult> Index(string vendorId = null)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == user.Id);

            var foodVendors = await _context.FoodVendors.ToListAsync();
            var selectedVendorId = vendorId ?? foodVendors.FirstOrDefault()?.Id;

            var foodItems = selectedVendorId != null
                ? await _context.Foods.Where(f => f.FoodVendorId == selectedVendorId && f.Status != FoodStatus.Deleted).ToListAsync()
                : new List<Food>();

            var cartItems = customer?.Cart?.Items.ToDictionary(i => i.FoodId, i => i.Quantity) ?? new Dictionary<int, int>();

            var model = new CustomerDashboardViewModel
            {
                FoodVendors = foodVendors,
                FoodItems = foodItems,
                SelectedVendorId = selectedVendorId,
                CartItems = cartItems,
                TotalPoints = await _runnerPointService.GetTotalPoints(user.Id)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CheckCart(int foodId)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .ThenInclude(i => i.Food)
                .FirstOrDefaultAsync(c => c.Id == user.Id);

            if (customer == null || !customer.Cart?.Items.Any() == true)
            {
                return Json(new { clearCart = false });
            }

            var food = await _context.Foods.FindAsync(foodId);
            var currentVendorId = customer.Cart.Items.FirstOrDefault()?.Food?.FoodVendorId;

            return Json(new { clearCart = currentVendorId != null && currentVendorId != food?.FoodVendorId });
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int foodId, bool clearCart)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == user.Id);

            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found." });
            }

            var cart = customer.Cart ?? new Cart
            {
                CustomerId = customer.Id,
                Customer = customer,
                Items = new List<CartItem>()
            };

            if (clearCart)
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
                await _context.SaveChangesAsync();
            }

            var food = await _context.Foods.FindAsync(foodId);
            if (food == null)
            {
                return Json(new { success = false, message = "Food item not found." });
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.FoodId == foodId);
            if (cartItem == null)
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    Cart = cart,
                    FoodId = foodId,
                    Food = food,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            if (customer.Cart == null)
            {
                _context.Carts.Add(cart);
                customer.Cart = cart;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, quantity = cart.Items.FirstOrDefault(ci => ci.FoodId == foodId)?.Quantity ?? 0 });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCartQuantity(int foodId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            var customer = await _context.Customers
                .Include(c => c.Cart)
                .ThenInclude(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == user.Id);

            if (customer == null)
            {
                return Json(new { success = false, message = "Customer not found." });
            }

            var cart = customer.Cart;
            if (cart == null)
            {
                return Json(new { success = false, message = "Cart not found." });
            }

            var cartItem = cart.Items.FirstOrDefault(ci => ci.FoodId == foodId);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Cart item not found." });
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, quantity = quantity });
        }
    }
}
