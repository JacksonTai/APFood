using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Admin;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.Admin}")]
    public class AdminController : Controller
    {
        private readonly IRegisterService _registrationService;
        private readonly APFoodContext _context;

        public AdminController(IRegisterService registrationService, APFoodContext context)
        {
            _registrationService = registrationService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .ThenInclude(i => i.Food)
                .ToListAsync();

            var processingCount = orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.Processing || o.Status == OrderStatus.Ready);
            var completedCount = orders.Count(o => o.Status == OrderStatus.Completed);
            var cancelledCount = orders.Count(o => o.Status == OrderStatus.Cancelled);
            var totalRevenue = orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Items.Sum(i => i.Quantity * i.Food.Price));

            var recentOrders = orders.Take(10).Select(o => new AdminOrderViewModel
            {
                OrderId = o.Id,
                CustomerName = o.Customer.FullName,
                FoodItems = string.Join("<br> ", o.Items.Select(i => $"{i.Quantity} x {i.Food.Name}")),
                DateTime = o.CreatedAt.ToString("dd MMM yyyy hh:mm tt"),
                TotalPrice = o.Items.Sum(i => i.Quantity * i.Food.Price),
                Status = o.Status.ToString()
            }).ToList();

            var model = new AdminDashboardViewModel
            {
                ProcessingCount = processingCount,
                CompletedCount = completedCount,
                CancelledCount = cancelledCount,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                OrderCounts = new Dictionary<OrderStatus, int>
                {
                    { OrderStatus.Pending, orders.Count(o => o.Status == OrderStatus.Pending) },
                    { OrderStatus.Processing, orders.Count(o => o.Status == OrderStatus.Processing) },
                    { OrderStatus.Ready, orders.Count(o => o.Status == OrderStatus.Ready) },
                    { OrderStatus.Completed, completedCount },
                    { OrderStatus.Cancelled, cancelledCount }
                }
            };

            return View(model);
        }

    }
}
