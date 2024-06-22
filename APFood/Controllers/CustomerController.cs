using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
