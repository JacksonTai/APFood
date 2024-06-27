using APFood.Constants;
using APFood.Constants.Order;
using APFood.Data;
using APFood.Models.Admin;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    [Authorize(Roles = $"{UserRole.Admin}")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
