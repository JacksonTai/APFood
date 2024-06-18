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

        public async Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status)
        {
            return await _context.Orders
                .Where(o => o.Status == status)
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.Id,
                    OrderTime = o.CreatedAt,
                    QueueNumber = o.QueueNumber,
                    DineInOption = o.DineInOption,
                    TotalPrice = o.Items.Sum(item => item.Quantity * item.Food.Price)
                })
                .ToListAsync();
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync()
        {
            Dictionary<OrderStatus, int> orderCounts = Enum.GetValues(typeof(OrderStatus))
               .Cast<OrderStatus>()
               .ToDictionary(status => status, status => 0);

            var dbCounts = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var dbCount in dbCounts)
            {
                if (orderCounts.ContainsKey(dbCount.Status))
                {
                    orderCounts[dbCount.Status] = dbCount.Count;
                }
            }

            return orderCounts;
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

            decimal deliveryFee = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .Select(p => p.DeliveryFee)
                .FirstOrDefaultAsync();

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
                    DeliveryFee = deliveryFee,
                    RunnerPointsRedeemed = 0,
                    Total = order.Items.Sum(item => item.Quantity * item.Food.Price) + deliveryFee
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
