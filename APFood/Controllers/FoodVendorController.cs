using APFood.Data;
using APFood.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using APFood.Areas.Identity.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using APFood.Constants.Order;
using APFood.Models.Order;
using APFood.Models.FoodVendor;
using Microsoft.Extensions.Logging;

namespace APFood.Controllers
{
    public class FoodVendorController : Controller
    {
        private readonly APFoodContext _context;
        private readonly UserManager<APFoodUser> _userManager;
        private readonly ILogger<FoodVendorController> _logger;

        public FoodVendorController(APFoodContext context, UserManager<APFoodUser> userManager, ILogger<FoodVendorController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var foodVendor = await _context.FoodVendors.FirstOrDefaultAsync(v => v.Id == user.Id);

            if (foodVendor == null)
            {
                _logger.LogWarning("Food vendor not found for user {UserId}", user.Id);
                return NotFound();
            }

            _logger.LogInformation("Food vendor found: {StoreName}", foodVendor.StoreName);

            var orders = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Food)
                .Include(o => o.Customer)
                .Where(o => o.Items.Any(oi => oi.Food.FoodVendorId == foodVendor.Id))
                .ToListAsync();

            _logger.LogInformation("Orders retrieved: {OrderCount}", orders.Count);

            var processingOrders = orders
                .Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Ready)
                .SelectMany(o => o.Items.Select(oi => new VendorOrderViewModel
                {
                    OrderId = o.Id.ToString(),
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.FullName,
                    FoodName = oi.Food.Name,
                    Quantity = oi.Quantity,
                    DateTime = o.CreatedAt.ToString("dd MMMM yyyy hh:mm tt"),
                    Price = oi.Food.Price,
                    Status = o.Status.ToString(),
                    DineInOption = o.DineInOption
                }))
                .ToList();

            _logger.LogInformation("Processing orders count: {Count}", processingOrders.Count);

            var completedOrders = orders
                .Where(o => o.Status == OrderStatus.Completed)
                .SelectMany(o => o.Items.Select(oi => new VendorOrderViewModel
                {
                    OrderId = o.Id.ToString(),
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.FullName,
                    FoodName = oi.Food.Name,
                    Quantity = oi.Quantity,
                    DateTime = o.CreatedAt.ToString("dd MMMM yyyy hh:mm tt"),
                    Price = oi.Food.Price,
                    Status = o.Status.ToString(),
                    DineInOption = o.DineInOption
                }))
                .ToList();

            _logger.LogInformation("Completed orders count: {Count}", completedOrders.Count);

            var cancelledOrders = orders
                .Where(o => o.Status == OrderStatus.Cancelled)
                .SelectMany(o => o.Items.Select(oi => new VendorOrderViewModel
                {
                    OrderId = o.Id.ToString(),
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer.FullName,
                    FoodName = oi.Food.Name,
                    Quantity = oi.Quantity,
                    DateTime = o.CreatedAt.ToString("dd MMMM yyyy hh:mm tt"),
                    Price = oi.Food.Price,
                    Status = o.Status.ToString(),
                    DineInOption = o.DineInOption
                }))
                .ToList();

            _logger.LogInformation("Cancelled orders count: {Count}", cancelledOrders.Count);

            var model = new FoodVendorDashboardViewModel
            {
                StoreName = foodVendor.StoreName,
                ProcessingCount = processingOrders.Select(o => o.OrderId).Distinct().Count(),
                CompletedCount = completedOrders.Select(o => o.OrderId).Distinct().Count(),
                CancelledCount = cancelledOrders.Select(o => o.OrderId).Distinct().Count(),
                TotalProfit = completedOrders.Sum(o => o.Price * o.Quantity),
                ProcessingOrders = processingOrders,
                CompletedOrders = completedOrders,
                CancelledOrders = cancelledOrders,
                OrderCounts = new Dictionary<OrderStatus, int>
        {
            { OrderStatus.Pending, orders.Count(o => o.Status == OrderStatus.Pending) },
            { OrderStatus.Processing, orders.Count(o => o.Status == OrderStatus.Processing) },
            { OrderStatus.Ready, orders.Count(o => o.Status == OrderStatus.Ready) },
            { OrderStatus.Completed, orders.Count(o => o.Status == OrderStatus.Completed) },
            { OrderStatus.Cancelled, orders.Count(o => o.Status == OrderStatus.Cancelled) }
        }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = newStatus;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
