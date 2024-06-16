using APFood.Data;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class CartService(APFoodContext context) : ICartService
    {
        private readonly APFoodContext _context = context;

        public async Task<Cart?> GetCartAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
        }

        public async Task<List<CartItem>> GetCartItemsAsync(string userId)
        {
            Cart? cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            return cart?.Items.ToList() ?? [];
        }

        public async Task AddItemAsync(string userId, Food food, int quantity)
        {
            Cart customerCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId) ?? throw new Exception("Cart not found");
            CartItem? existingItem = customerCart?.Items.FirstOrDefault(ci => ci.FoodId == food.Id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                customerCart?.Items.Add(new CartItem { FoodId = food.Id, Quantity = quantity, Cart = customerCart, Food = food });
            }
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int itemId, int newQuantity)
        {
            CartItem cartItem = await _context.CartItems
                .Include(ci => ci.Food)
                .FirstOrDefaultAsync(ci => ci.Id == itemId) ?? throw new Exception("Cart Item not found");
            cartItem.Quantity = newQuantity;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int itemId)
        {
            CartItem cartItem = await _context.CartItems.FindAsync(itemId) ?? throw new Exception("Cart Item not found");
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            Cart? cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            _context.CartItems.RemoveRange(cart?.Items);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalAsync(string userId)
        {
            Cart? cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.CustomerId == userId);
            return cart?.Items.Sum(ci => ci.Food.Price * ci.Quantity) ?? 0;
        }

    }
}
