using APFood.Constants;
using APFood.Constants.Order;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.Customer},{UserRole.Admin}")]
    [Route("[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IRunnerPointService _runnerPointService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            IRunnerPointService runnerPointService,
            ILogger<OrderController> logger
        )
        {
            _orderService = orderService;
            _runnerPointService = runnerPointService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus status = OrderStatus.Pending)
        {
            string userId = GetUserId();
            bool isAdmin = User.IsInRole(UserRole.Admin);
            try
            {
                List<OrderListViewModel> orders;
                Dictionary<OrderStatus, int> orderCounts;
                if (isAdmin)
                {
                    orders = await _orderService.GetOrdersByStatusAdminAsync(status);
                    orderCounts = await _orderService.GetOrderCountsAdminAsync();
                }
                else
                {
                    orders = await _orderService.GetOrdersByStatusAsync(status, userId);
                    orderCounts = await _orderService.GetOrderCountsAsync(userId);
                }

                return View(new OrderViewModel
                {
                    OrderList = orders,
                    OrderCounts = orderCounts,
                    CurrentStatus = status,
                    TotalPointsSpent = isAdmin ? 0 : await _runnerPointService.GetTotalSpent(userId),
                    IsAdmin = isAdmin
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            bool isAdmin = User.IsInRole(UserRole.Admin);
            try
            {
                if (isAdmin)
                {
                    OrderDetailViewModel? orderDetail = await _orderService.GetOrderDetailAdminAsync(id);
                    return orderDetail == null ? NotFound() : View(orderDetail);
                }
                else
                {
                    OrderDetailViewModel? orderDetail = await _orderService.GetOrderDetailAsync(id, GetUserId());
                    return orderDetail == null ? NotFound() : View(orderDetail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading the order detail for order {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("ReceiveOrder")]
        public async Task<IActionResult> ReceiveOrder(int orderId)
        {
            try
            {
                await _orderService.ReceiveOrder(orderId);
                return Redirect(Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while receiving the order {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                await _orderService.CancelOrder(orderId);
                return Redirect(Request.Headers.Referer.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cancelling the order {OrderId}", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User not authenticated.");
        }
    }
}
