using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<APFoodUser> _userManager;
        private readonly APFoodContext _context;

        public AdminController(UserManager<APFoodUser> userManager, APFoodContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult ManageVendors()
        {
            var vendors = _userManager.GetUsersInRoleAsync(UserRole.FoodVendor).Result;
            return View(vendors);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVendorStatus(string vendorId, string status)
        {
            var vendor = await _userManager.FindByIdAsync(vendorId);
            if (vendor != null)
            {
                // Update vendor status logic
                await _userManager.UpdateAsync(vendor);
            }
            return RedirectToAction("ManageVendors");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(string vendorId)
        {
            var vendor = await _userManager.FindByIdAsync(vendorId);
            if (vendor != null)
            {
                // Delete vendor and related data
                await _userManager.DeleteAsync(vendor);
            }
            return RedirectToAction("ManageVendors");
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor(string email, string password)
        {
            var vendor = new APFoodUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(vendor, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(vendor, UserRole.FoodVendor);
            }
            return RedirectToAction("ManageVendors");
        }
        public IActionResult OrderHistory()
        {
            var orders = _context.Orders.Include(o => o.Payment).ToList();
            return View(orders);
        }
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders.Include(o => o.Payment).FirstOrDefault(o => o.Id == id);
            return View(order);
        }
        public IActionResult DeliveryTasks()
        {
            var tasks = _context.DeliveryTasks.ToList();
            return View(tasks);
        }
    }


}
