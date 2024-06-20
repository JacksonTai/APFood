using APFood.Areas.Identity.Data;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;

namespace APFood.Services.Contract
{
    public interface ICartService
    {
        Task CreateCustomerCart(Customer customer);
        Task<CartViewModel> CheckoutCart(string userId, CartFormModel cartForm);
        Task<CartViewModel> GetCartViewAsync(string userId, CartFormModel cartForm);
        Task<CartViewModel> GetCartViewAsync(string userId);
        Task<Cart?> GetCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<List<CartItem>> GetCartItemsAsync(string userId);
        Task AddItemAsync(string userId, Food food, int quantity);
        Task<UpdateQuantityResponseModel> UpdateQuantityAsync(Cart cart, UpdateQuantityRequestModel updateQuantityRequest);
        Task<UpdateRunnerPointsResponseModel> UpdateRunnerPointsAsync(Cart cart, bool isUsingRunnerPoints);
        Task<UpdateDiningOptionResponseModel> UpdateDiningOption(Cart cart, DineInOption dineInOption);
        Task<RemoveItemResponseModel> RemoveItemAsync(Cart cart, int itemId);
        Task ClearCartAsync(string userId);
    }
}
