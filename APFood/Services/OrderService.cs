using APFood.Constants.Order;
using APFood.Controllers;
using APFood.Data;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

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
                Customer = cart.Customer,
                Items = cart.Items.Select(ci => new OrderItem
                {
                    FoodId = ci.FoodId,
                    Food = ci.Food,
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

        public async Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId)
        {
            Order? order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return null;
            }

            OrderDetailViewModel orderDetailViewModel = new()
            {
                OrderId = order.Id,
                Status = order.Status,
                OrderTime = order.CreatedAt,
                DineInOption = order.DineInOption,
                QueueNumber = order.QueueNumber,
                Items = order.Items,
                OrderSummary = new OrderSummaryModel
                {
                    Subtotal = order.Items.Sum(item => item.Quantity * item.Food.Price),
                    DeliveryFee = 0,
                    RunnerPointsRedeemed = 0,
                    Total = order.Items.Sum(item => item.Quantity * item.Food.Price)
                }
            };

            Data.DeliveryTask? deliveryTask = await _context.DeliveryTasks
                    .FirstOrDefaultAsync(o => o.Id == orderId);

            if (deliveryTask != null)
            {
                orderDetailViewModel.DeliveryLocation = deliveryTask.Location;
                orderDetailViewModel.DeliveryStatus = deliveryTask.Status;

                RunnerDeliveryTask? runnerDeliveryTasks = await _context.RunnerDeliveryTasks
                       .Include(rdt => rdt.Runner)
                       .Where(rdt => rdt.DeliveryTaskId == deliveryTask.Id)
                       .Where(rdt => rdt.Status != Constants.DeliveryStatus.Cancelled)
                       .FirstOrDefaultAsync();

                if (runnerDeliveryTasks != null)
                {
                    orderDetailViewModel.Runner = runnerDeliveryTasks.Runner.FullName;
                }
            }
            return orderDetailViewModel;
        }
    }
}
