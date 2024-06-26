﻿using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;

namespace APFood.Services.Contract
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(Cart cart, DineInOption dineInOption);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task ReceiveOrder(int orderId);
        Task CancelOrder(int orderId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status, string userId);
        Task<List<OrderListViewModel>> GetOrdersByStatusAdminAsync(OrderStatus status);
        Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync(string userId);
        Task<Dictionary<OrderStatus, int>> GetOrderCountsAdminAsync();
        Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId, string userId);
        Task<OrderDetailViewModel?> GetOrderDetailAdminAsync(int orderId);
        OrderSummaryModel CalculateOrderSummary(Cart cart, CartFormModel cartForm);
    }
}
