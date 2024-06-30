using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;

namespace APFood.Services
{
    public class OrderService(
        APFoodContext context,
        IDeliveryTaskService deliveryTaskService
        ) : IOrderService
    {
        private readonly APFoodContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;

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

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            Order order = await _context.Orders.FindAsync(orderId) ?? throw new Exception("Order not found");
            order.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task ReceiveOrder(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Completed);
            DeliveryTask deliveryTask = await _context.DeliveryTasks
                .Include(dt => dt.RunnerDeliveryTasks)
                .ThenInclude(rdt => rdt.Runner)
                .Where(deliveryTask => deliveryTask.Status == DeliveryStatus.Delivered)
                .FirstOrDefaultAsync(dt => dt.OrderId == orderId) ?? throw new Exception("Delivery task not found");

            // Reward runner points
            List<RunnerDeliveryTask> runnerDeliveryTasks = deliveryTask.RunnerDeliveryTasks
                ?? throw new Exception("Runner delivery task not found");
            Customer? runner = runnerDeliveryTasks
                .Where(rdt => rdt.Status == DeliveryStatus.Delivered)
                .First().Runner ?? throw new Exception("Runner not found");
            runner.Points += OrderConstants.RunnerPointsPerDelivery;
            await _context.SaveChangesAsync();
        }

        public async Task CancelOrder(int orderId)
        {
            await UpdateOrderStatusAsync(orderId, OrderStatus.Cancelled);
            DeliveryTask? deliveryTask = await _context.DeliveryTasks
                .Where(deliveryTask => deliveryTask.Status == DeliveryStatus.Pending)
                .FirstOrDefaultAsync(dt => dt.OrderId == orderId);
            if (deliveryTask != null)
            {
                await _deliveryTaskService.UpdateDeliveryStatusAsync(deliveryTask.Id, DeliveryStatus.Cancelled);
            }

            // Refund runner points
            Payment payment = await _context.Payments
                .Include(p => p.Order)
                .ThenInclude(o => o.Customer)
                .FirstOrDefaultAsync(p => p.OrderId == orderId) ?? throw new Exception("Payment not found");
            if (payment.RunnerPointsUsed > 0)
            {
                payment.Order.Customer.Points += payment.RunnerPointsUsed;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status, string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Food)
                .Where(o => o.Status == status)
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.Id,
                    OrderTime = o.CreatedAt,
                    QueueNumber = o.QueueNumber,
                    DineInOption = o.DineInOption,
                    OrderStatus = o.Status,
                    TotalPaid = _context.Payments
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => p.Total)
                        .FirstOrDefault(),
                    IsReceivableOrder = IsReceivableOrder(o.Status, _context.DeliveryTasks
                        .Where(dt => dt.OrderId == o.Id)
                        .Select(dt => dt.Status)
                        .FirstOrDefault()),
                    IsCancellableOrder = o.Status == OrderStatus.Pending
                }).ToListAsync();
        }

        public async Task<List<OrderListViewModel>> GetOrdersByStatusAdminAsync(OrderStatus status)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Food)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.Id,
                    OrderTime = o.CreatedAt,
                    QueueNumber = o.QueueNumber,
                    DineInOption = o.DineInOption,
                    OrderStatus = o.Status,
                    TotalPaid = _context.Payments
                        .Where(p => p.OrderId == o.Id)
                        .Select(p => p.Total)
                        .FirstOrDefault(),
                    IsReceivableOrder = IsReceivableOrder(o.Status, _context.DeliveryTasks
                        .Where(dt => dt.OrderId == o.Id)
                        .Select(dt => dt.Status)
                        .FirstOrDefault()),
                    IsCancellableOrder = o.Status == OrderStatus.Pending
                }).ToListAsync();
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync(string userId)
        {
            Dictionary<OrderStatus, int> orderCounts = Enum.GetValues(typeof(OrderStatus))
               .Cast<OrderStatus>()
               .ToDictionary(status => status, status => 0);

            var dbCounts = await _context.Orders
                .Where(o => o.CustomerId == userId)
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

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountsAdminAsync()
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

        public async Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId, string userId)
        {
            Order order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Food)
                .Where(o => o.CustomerId == userId)
                .FirstOrDefaultAsync(o => o.Id == orderId) ?? throw new Exception("Order not found");

            Payment? payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
              ?? throw new Exception("Payment not found");

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
                    DeliveryFee = payment.DeliveryFee,
                    RunnerPointsRedeemed = payment.RunnerPointsUsed,
                    Total = payment.Total
                },
                DeliveryLocation = deliveryTask?.Location,
                DeliveryStatus = deliveryTask?.Status,
                Runner = runnerDeliveryTask?.Runner?.FullName,
                IsReceivableOrder = IsReceivableOrder(order.Status, deliveryTask?.Status),
                IsCancellableOrder = order.Status == OrderStatus.Pending
            };

            return orderDetailViewModel;
        }

        public async Task<OrderDetailViewModel?> GetOrderDetailAdminAsync(int orderId)
        {
            Order order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Food)
                .FirstOrDefaultAsync(o => o.Id == orderId) ?? throw new Exception("Order not found");

            Payment? payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
              ?? throw new Exception("Payment not found");

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
                    DeliveryFee = payment.DeliveryFee,
                    RunnerPointsRedeemed = payment.RunnerPointsUsed,
                    Total = payment.Total
                },
                DeliveryLocation = deliveryTask?.Location,
                DeliveryStatus = deliveryTask?.Status,
                Runner = runnerDeliveryTask?.Runner?.FullName,
                IsReceivableOrder = IsReceivableOrder(order.Status, deliveryTask?.Status),
                IsCancellableOrder = order.Status == OrderStatus.Pending
            };

            return orderDetailViewModel;
        }

        public OrderSummaryModel CalculateOrderSummary(Cart cart, CartFormModel cartForm)
        {
            decimal subtotal = cart.Items.Sum(ci => ci.Food.Price * ci.Quantity);
            decimal deliveryFee = cartForm.DineInOption == DineInOption.Delivery ? OrderConstants.DeliveryFee : 0;
            decimal runnerPointsRedeemed = Math.Min(cart.Customer.Points, deliveryFee + subtotal);
            decimal total = Math.Max(subtotal + deliveryFee - (cartForm.IsUsingRunnerPoints ? runnerPointsRedeemed : 0), 0);
            return new OrderSummaryModel
            {
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                RunnerPointsRedeemed = runnerPointsRedeemed,
                Total = total
            };
        }

        private static bool IsReceivableOrder(OrderStatus orderStatus, DeliveryStatus? deliveryStatus)
        {
            return orderStatus == OrderStatus.Ready &&
                   (deliveryStatus != null && deliveryStatus == DeliveryStatus.Delivered);
        }
    }
}
