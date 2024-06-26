using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace APFood.Controllers
{
    public class AdminOrderController : Controller
    {
        private readonly APFoodContext _context;
        private readonly IOrderService _orderService;
        private readonly ILogger<AdminOrderController> _logger;

        public AdminOrderController(APFoodContext context, IOrderService orderService, ILogger<AdminOrderController> logger)
        {
            _context = context;
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(OrderStatus status = OrderStatus.Pending)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            var orderCounts = await _orderService.GetOrderCountsAsync();

            var model = new OrderViewModel
            {
                OrderList = orders,
                OrderCounts = orderCounts,
                CurrentStatus = status
            };

            return View(model);
        }
    }
}
