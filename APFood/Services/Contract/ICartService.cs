using APFood.Areas.Identity.Data;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;

namespace APFood.Services.Contract
{
    public interface ICartService
    {
        Task CreateCustomerCart(Customer customer);
        Task<CartViewModel> CheckoutCart(Cart cart, CartFormModel cartForm);
        Task<CartViewModel> GetCartViewAsync(Cart cart, CartFormModel cartForm);
        Task<CartViewModel> GetCartViewAsync(Cart cart);
        Task<Cart?> GetCartAsyncWithTracking(string userId);
        Task<Cart?> GetCartAsyncNoTracking(string userId);
         Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task AddItemAsync(string userId, Food food, int quantity);
        Task<UpdateQuantityResponseModel> UpdateQuantityAsync(Cart cart, UpdateQuantityRequestModel updateQuantityRequest);
        UpdateRunnerPointsResponseModel UpdateRunnerPoints(Cart cart, bool isUsingRunnerPoints);
        UpdateDiningOptionResponseModel UpdateDiningOption(Cart cart, DineInOption dineInOption);
        Task<RemoveItemResponseModel> RemoveItemAsync(Cart cart, int itemId);
        Task ClearCartAsync(string userId);
    }
}
