using APFood.Data;
using APFood.Models.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly APFoodContext _context;

        public CartController(APFoodContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var customerCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (customerCart == null)
            {
                return NotFound();
            }

            return View(customerCart.Items.ToList());
        }

        public IActionResult AddItem()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveItem(int itemId)
        {
            var cartItem = await _context.CartItems.FindAsync(itemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            // Calculate total items
            var totalItems = cart.Items.Sum(ci => ci.Quantity);

            // Calculate total price, ensuring ci.Food is not null
            var totalPrice = cart.Items.Where(ci => ci.Food != null)
                                       .Sum(ci => ci.Quantity * ci.Food.Price);

            return Json(new
            {
                totalItems = totalItems.ToString("C"),
                totalPrice = totalPrice.ToString("C")
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemQuantityRequest request)
        {
            var cartItem = await _context.CartItems
           .Include(ci => ci.Food)
           .FirstOrDefaultAsync(ci => ci.Id == request.ItemId);

            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Quantity = request.NewQuantity;
            await _context.SaveChangesAsync();

            var itemPrice = cartItem.Food.Price * cartItem.Quantity;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);

            if (cart == null)
            {
                return NotFound();
            }

            var totalItems = cart.Items.Sum(ci => ci.Quantity);
            var totalPrice = cart.Items.Sum(ci => ci.Quantity * ci.Food.Price);

            return Json(new
            {
                itemPrice = itemPrice.ToString("C"),
                totalItems = totalItems,
                totalPrice = totalPrice.ToString("C")
            });
        }

        public IActionResult Checkout()
        {
            return View();
        }



    }
}
