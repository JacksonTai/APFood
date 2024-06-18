using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Order;

namespace APFood.Services.Contract
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(Cart cart, DineInOption dineInOption);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId);
    }
}
