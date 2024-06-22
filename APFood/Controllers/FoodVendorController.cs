using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class FoodVendorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
