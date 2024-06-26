using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
