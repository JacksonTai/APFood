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

        [HttpPost]
        public async Task<IActionResult> ReceiveOrder(int orderId)
        {
            bool orderStatusresult = await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Completed);
            bool deliveryStatusResult = await _orderService.UpdateOrderDeliveryStatusAsync(orderId, DeliveryStatus.Delivered);
            return (orderStatusresult && deliveryStatusResult) ? Redirect(Request.Headers.Referer.ToString()) : BadRequest();
        }

    }
}
