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
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<bool> UpdateOrderDeliveryStatusAsync(int orderId, DeliveryStatus newStatus);
        Task<bool> ReceiveOrder(int orderId);
        Task<bool> CancelOrder(int orderId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status);
        Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync();
        Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId);
        OrderSummaryModel CalculateOrderSummary(Cart cart, CartFormModel cartForm);
    }
}
