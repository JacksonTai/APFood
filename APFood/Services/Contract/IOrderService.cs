using APFood.Constants.Order;
using APFood.Data;

namespace APFood.Services.Contract
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(Cart cart, DineInOption dineInOption);
        Task<Order?> GetOrderByIdAsync(int orderId);

    }
}
