using APFood.Constants;
using APFood.Constants.Order;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APFood.Controllers
{
    [Authorize(Roles = UserRole.Customer)]
    [Route("[controller]")]
    public class OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger
        ) : Controller
    {
        private readonly IOrderService _orderService = orderService;
        private readonly ILogger<OrderController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus status = OrderStatus.Pending)
        {
            try
            {
                List<OrderListViewModel> orders = await _orderService.GetOrdersByStatusAsync(status, GetUserId());
                Dictionary<OrderStatus, int> orderCounts = await _orderService.GetOrderCountsAsync(GetUserId());
                return View(new OrderViewModel
                {
                    OrderList = orders,
                    OrderCounts = orderCounts,
                    CurrentStatus = status
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
            try
            {
                OrderDetailViewModel? orderDetail = await _orderService.GetOrderDetailAsync(id, GetUserId());
                return orderDetail == null ? NotFound() : View(orderDetail);
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
