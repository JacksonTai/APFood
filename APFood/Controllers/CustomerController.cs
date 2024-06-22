using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using APFood.Data;
using APFood.Models.Customer;
using Microsoft.EntityFrameworkCore;

namespace APFood.Controllers
{
    public class CustomerController : Controller
    {
        private readonly APFoodContext _context;

        public CustomerController(APFoodContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var foodVendors = await _context.FoodVendors.ToListAsync();
            var selectedVendorId = foodVendors.FirstOrDefault()?.Id;

            var foodItems = selectedVendorId != null
                ? await _context.Foods.Where(f => f.FoodVendorId == selectedVendorId).ToListAsync()
                : new List<Food>();

            var model = new CustomerDashboardViewModel
            {
                FoodVendors = foodVendors,
                FoodItems = foodItems,
                SelectedVendorId = selectedVendorId
            };

            return View(model);
        }

        public async Task<IActionResult> GetFoodItems(string vendorId)
        {
            var foodItems = await _context.Foods.Where(f => f.FoodVendorId == vendorId).ToListAsync();
            return PartialView("_FoodItemsPartial", foodItems);
        }
    }
}
