using APFood.Constants;
using APFood.Constants.Order;
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

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            Order? order = await _context.Orders.FindAsync(orderId);

            if (order == null)
            {
                return false;
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateOrderDeliveryStatusAsync(int orderId, DeliveryStatus newStatus)
        {
            DeliveryTask? deliveryTask = await _context.DeliveryTasks
                .FirstOrDefaultAsync(dt => dt.OrderId == orderId);
            if (deliveryTask == null)
            {
                return false;
            }

            if (newStatus == DeliveryStatus.Delivered)
            {
                RunnerDeliveryTask? runnerDeliveryTask = await _context.RunnerDeliveryTasks
                    .Where(rdt => rdt.Status == DeliveryStatus.Accepted)
                    .FirstOrDefaultAsync(rdt => rdt.DeliveryTaskId == deliveryTask.Id);
                if (runnerDeliveryTask == null)
                {
                    return false;
                }
                runnerDeliveryTask.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            deliveryTask.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReceiveOrder(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Completed);
        }

        public async Task<bool> CancelOrder(int orderId)
        {
            DeliveryTask? deliveryTask = await _context.DeliveryTasks
                .FirstOrDefaultAsync(dt => dt.OrderId == orderId);

            if (deliveryTask != null)
            {
               bool result = await UpdateOrderDeliveryStatusAsync(orderId, DeliveryStatus.Cancelled);
               if (!result)
                {
                }
            }
            return await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                                 .Include(o => o.Items)
                                 .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var ordersQuery = _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Food)
                .Where(o => o.Status == status)
                .Select(o => new
                {
                    o.Id,
                    o.CreatedAt,
                    o.QueueNumber,
                    o.DineInOption,
                    o.Items,
                    o.Status,
                    DeliveryTask = _context.DeliveryTasks
                                          .FirstOrDefault(dt => dt.OrderId == o.Id)
                });

            var orders = await ordersQuery.ToListAsync();

            return orders.Select(o => new OrderListViewModel
            {
                OrderId = o.Id,
                OrderTime = o.CreatedAt,
                QueueNumber = o.QueueNumber,
                DineInOption = o.DineInOption,
                TotalPrice = o.Items.Sum(item => item.Quantity * item.Food.Price),
                OrderStatus = o.Status,
                CanShowReceivedButton = CanShowReceivedButton(o.Status, o.DeliveryTask?.Status),
                CanShowCancelButton = o.Status == OrderStatus.Pending
            }).ToList();
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

            DeliveryTask? deliveryTask = await _context.DeliveryTasks
                .FirstOrDefaultAsync(dt => dt.OrderId == orderId);

            RunnerDeliveryTask? runnerDeliveryTask = deliveryTask != null
                ? await _context.RunnerDeliveryTasks
                    .Include(rdt => rdt.Runner)
                    .Where(rdt => rdt.DeliveryTaskId == deliveryTask.Id)
                    .Where(rdt => rdt.Status != DeliveryStatus.Cancelled)
                    .FirstOrDefaultAsync()
                : null;

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
                },
                DeliveryLocation = deliveryTask?.Location,
                DeliveryStatus = deliveryTask?.Status,
                Runner = runnerDeliveryTask?.Runner.FullName,
                CanShowReceivedButton = CanShowReceivedButton(order.Status, deliveryTask?.Status),
                CanShowCancelButton = order.Status == OrderStatus.Pending
            };

            return orderDetailViewModel;
        }

        private static bool CanShowReceivedButton(OrderStatus orderStatus, DeliveryStatus? deliveryStatus)
        {
            return orderStatus == OrderStatus.Ready &&
                   (deliveryStatus != null && deliveryStatus == DeliveryStatus.Delivered);
        }
    }
}
