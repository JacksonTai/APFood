using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class AdminController : Controller
    {
        {
            var tasks = _context.DeliveryTasks.ToList();
            return View(tasks);
        }
    }


}
