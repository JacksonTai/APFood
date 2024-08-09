using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Cart;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace APFood.Services
{
    public class OrderService(
        APFoodContext context,
        IDeliveryTaskService deliveryTaskService,
        HttpClient httpClient
        ) : IOrderService
    {
        private readonly APFoodContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IDeliveryTaskService _deliveryTaskService = deliveryTaskService;
        private readonly HttpClient _httpClient = httpClient;

        public async Task<CreateOrderResponseModel> CreateOrder(Cart cart, DineInOption dineInOption)
        {
            var createOrderRequest = new CreateOrderRequestModel
            {
                CartId = cart.Id,
                DineInOption = dineInOption
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(createOrderRequest, jsonOptions), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("order", jsonContent);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<CreateOrderResponseModel>(responseContent, jsonOptions)
                                ?? throw new Exception("An error occurred while creating order");

            return responseModel;
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var response = await _httpClient.PutAsJsonAsync($"order/{orderId}?status={newStatus}", newStatus);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var response = await _httpClient.GetAsync($"order/{orderId}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            var responseModel = JsonSerializer.Deserialize<Order>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                }) ?? throw new Exception("An error occurred while getting order by id");

            return responseModel;
        }

        public async Task<List<OrderListViewModel>> GetOrdersByStatusAsync(OrderStatus status, string? customerId)
        {
            string url = string.IsNullOrEmpty(customerId) ? $"order?status={status}" : $"order?status={status}&customerId={customerId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<OrderListResponseModel>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }) ?? throw new Exception("An error occurred while getting orders by status");
            return responseModel.orderListViewModels;
        }

        public async Task<Dictionary<OrderStatus, int>> GetOrderCountsAsync(string? customerId)
        {
            string url = string.IsNullOrEmpty(customerId) ? "order/counts" : $"order/counts?customerId={customerId}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var statusCount = JsonSerializer.Deserialize<OrderStatusCount>(responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new Exception("Failed to deserialize the response.");

            var statusDictionary = new Dictionary<OrderStatus, int>
            {
                { OrderStatus.Pending, statusCount.Pending },
                { OrderStatus.Processing, statusCount.Processing },
                { OrderStatus.Ready, statusCount.Ready },
                { OrderStatus.Completed, statusCount.Completed },
                { OrderStatus.Cancelled, statusCount.Cancelled }
            };

            return statusDictionary;
        }

        public OrderSummaryModel CalculateOrderSummary(Cart cart, CartFormModel cartForm)
        {
            var response = _httpClient.PostAsJsonAsync("order/summary", new OrderSummaryRequestModel()
            {
                Cart = cart,
                IsUsingRunnerPoints = cartForm.IsUsingRunnerPoints,
                DineInOption = cartForm.DineInOption
            }, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNameCaseInsensitive = true
            }).Result;

            response.EnsureSuccessStatusCode();

            return response.Content.ReadFromJsonAsync<OrderSummaryModel>(new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                PropertyNameCaseInsensitive = true
            }).Result ?? throw new Exception("An error occurs while calculating the order summary");
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

        public async Task<OrderDetailViewModel?> GetOrderDetailAsync(int orderId)
        {
            Order order = await GetOrderByIdAsync(orderId) ?? 
                throw new Exception("Order not found");

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

        private static bool IsReceivableOrder(OrderStatus orderStatus, DeliveryStatus? deliveryStatus)
        {
            return orderStatus == OrderStatus.Ready &&
                   (deliveryStatus != null && deliveryStatus == DeliveryStatus.Delivered);
        }

    }
}
