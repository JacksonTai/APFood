using APFood.Constants;
using APFood.Constants.Order;
using APFood.Models.Order;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    [Route("[controller]")]
    public class OrderController(IOrderService orderService) : Controller
    {
        private readonly IOrderService _orderService = orderService;

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus status = OrderStatus.Pending)
        {
            List<OrderListViewModel> orders = await _orderService.GetOrdersByStatusAsync(status);
            Dictionary<OrderStatus, int> orderCounts = await _orderService.GetOrderCountsAsync();
            return View(new OrderViewModel
            {
                OrderList = orders,
                OrderCounts = orderCounts,
                CurrentStatus = status
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            OrderDetailViewModel? orderDetail = await _orderService.GetOrderDetailAsync(id);
            return orderDetail == null ? NotFound() : View(orderDetail);
        }

        [HttpPost("ReceiveOrder")]
        public async Task<IActionResult> ReceiveOrder(int orderId)
        {
            bool result = await _orderService.ReceiveOrder(orderId);
            return result ? Redirect(Request.Headers.Referer.ToString()) : BadRequest();
        }

        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            bool result = await _orderService.CancelOrder(orderId);
            return result ? Redirect(Request.Headers.Referer.ToString()) : BadRequest();
        }

    }
}
