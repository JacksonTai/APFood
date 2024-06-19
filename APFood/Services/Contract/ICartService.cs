using APFood.Areas.Identity.Data;
using APFood.Data;
using APFood.Models.Cart;

namespace APFood.Services.Contract
{
    public interface ICartService
    {
        Task CreateCustomerCart(Customer customer);
        Task<Cart?> GetCartAsync(string userId);
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task AddItemAsync(string userId, Food food, int quantity);
        Task UpdateQuantityAsync(int itemId, int newQuantity);
        Task RemoveItemAsync(int itemId);
        Task ClearCartAsync(string userId);
        Task<decimal> GetTotalAsync(string userId);
    }
}
