using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class DeliveryTask : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
