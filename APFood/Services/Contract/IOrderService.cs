﻿using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Order;

namespace APFood.Services.Contract
{
    public interface IOrderService
    {
        Task<Order> CreateOrder(Cart cart, DineInOption dineInOption);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<bool> UpdateOrderDeliveryStatusAsync(int orderId, DeliveryStatus newStatus);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status);
        Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync();
        Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId);

    }
}
