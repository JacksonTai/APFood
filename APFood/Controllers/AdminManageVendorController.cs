using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Data;
using APFood.Models.Admin;
using APFood.Models.Register;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APFood.Controllers
{
    public class AdminManageVendorController : Controller
    {
        private readonly IRegisterService _registrationService;
        private readonly APFoodContext _context;

        public AdminManageVendorController(IRegisterService registrationService, APFoodContext context)
        {
            _registrationService = registrationService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vendors = await _context.FoodVendors
                .Select(v => new FoodVendorViewModel
                {
                    Id = v.Id,
                    StoreName = v.StoreName,
                    Email = v.Email
                }).ToListAsync();

            return View(vendors);
        }

        public IActionResult CreateVendor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVendor(FoodVendorRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new FoodVendor
                {
                    StoreName = model.StoreName,
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await _registrationService.RegisterUserAsync(user, model, UserRole.FoodVendor);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> EditVendor(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vendor = await _context.FoodVendors.FindAsync(id);
            if (vendor == null)
            {
                return NotFound();
            }

            var model = new FoodVendorViewModel
            {
                Id = vendor.Id,
                StoreName = vendor.StoreName,
                Email = vendor.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditVendor(FoodVendorViewModel model)
        {
            if (ModelState.IsValid)
            {
                var vendor = await _context.FoodVendors.FindAsync(model.Id);
                if (vendor == null)
                {
                    return NotFound();
                }

                vendor.StoreName = model.StoreName;
                vendor.Email = model.Email;

                _context.Update(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(string id)
        {
            var vendor = await _context.FoodVendors.FindAsync(id);
            if (vendor != null)
            {
                var foodItems = _context.Foods.Where(f => f.FoodVendorId == vendor.Id);
                _context.Foods.RemoveRange(foodItems);

                await _context.SaveChangesAsync();

                _context.FoodVendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
