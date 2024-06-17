using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


    }
}
