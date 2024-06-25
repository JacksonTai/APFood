using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Data;
using APFood.Models.Admin;
using APFood.Models.Register;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace APFood.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRegisterService _registrationService;
        private readonly APFoodContext _context;

        public AdminController(IRegisterService registrationService, APFoodContext context)
        {
            _registrationService = registrationService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ManageVendors()
        {
            var vendors = await _context.FoodVendors
                .Select(v => new FoodVendorViewModel
                {
                    Id = v.Id,
                    StoreName = v.StoreName,
                    UserName = v.UserName,
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
                    return RedirectToAction(nameof(ManageVendors));
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
                UserName = vendor.UserName,
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
                vendor.UserName = model.UserName;
                vendor.Email = model.Email;

                _context.Update(vendor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ManageVendors));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVendor(string id)
        {
            var vendor = await _context.FoodVendors.FindAsync(id);
            if (vendor != null)
            {
                _context.FoodVendors.Remove(vendor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageVendors));
        }
    }
}
