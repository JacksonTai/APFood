using APFood.Constants.Order;
using APFood.Data;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APFood.Services
{
    public class OrderService(APFoodContext context) : IOrderService
    {
        private readonly APFoodContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<Order> CreateOrder(Cart cart, DineInOption dineInOption)
        {
            Order order = new()
            {
                CustomerId = cart.CustomerId,
                Items = cart.Items.Select(ci => new OrderItem
                {
                    FoodId = ci.FoodId,
                    Quantity = ci.Quantity
                }).ToList(),
                Status = OrderStatus.Pending,
                DineInOption = dineInOption,
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                                 .Include(o => o.Items)
                                 .FirstOrDefaultAsync(o => o.Id == orderId);
        }
    }
}
