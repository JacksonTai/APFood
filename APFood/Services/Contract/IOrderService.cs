using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;

namespace APFood.Services.Contract
{
    public interface IOrderService
    {
        Task<CreateOrderResponseModel> CreateOrder(Cart cart, DineInOption dineInOption);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status, string? customerId);
        Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync(string? customerId);
        OrderSummaryModel CalculateOrderSummary(Cart cart, CartFormModel cartForm);


        Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId);
        Task ReceiveOrder(int orderId);
        Task CancelOrder(int orderId);
        
    }
}
