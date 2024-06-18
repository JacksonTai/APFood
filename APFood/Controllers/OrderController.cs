using APFood.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    [Route("[controller]")]
    public class OrderController(IOrderService orderService) : Controller
    {
        private readonly IOrderService _orderService = orderService;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            var orderDetails = await _orderService.GetOrderDetailAsync(id);
            if (orderDetails == null)
            {
                return NotFound();
            }
            return View(orderDetails);
        }
    }
}
