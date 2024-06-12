using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class LoginController : Controller
    {

        [HttpGet]
        public IActionResult Customer()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Runner()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FoodVendor()
        {
            return View();
        }

    }
}
