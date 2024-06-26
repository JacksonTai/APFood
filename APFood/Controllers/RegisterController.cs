using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Models.Register;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class RegisterController(
        IRegisterService registrationService,
        ICartService cartService
        ) : Controller
    {
        private readonly IRegisterService _registrationService = registrationService;
        private readonly ICartService _cartService = cartService;

        [HttpPost]
        public async Task<IActionResult> Customer(CustomerRegistrationModel customerRegistrationModel)
        {
            return await RegisterUser(new Customer { FullName = customerRegistrationModel.FullName },
                customerRegistrationModel, UserRole.Customer);
        }

        [HttpPost]
        public async Task<IActionResult> FoodVendor(FoodVendorRegistrationModel foodVendorRegistrationModel)
        {
            return await RegisterUser(new FoodVendor { StoreName = foodVendorRegistrationModel.StoreName },
                foodVendorRegistrationModel, UserRole.FoodVendor);
        }

        [HttpPost]
        public async Task<IActionResult> Admin(RegistrationModel registrationModel)
        {
            return await RegisterUser(new Admin(), registrationModel, UserRole.Admin);
        }

        [HttpGet]
        public IActionResult Customer()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FoodVendor()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Admin()
        {
            return View();
        }

        private async Task<IActionResult> RegisterUser(APFoodUser user, RegistrationModel registrationModel, string role)
        {
            ArgumentNullException.ThrowIfNull(registrationModel);

            if (!ModelState.IsValid)
            {
                return View(registrationModel);
            }

            var result = await _registrationService.RegisterUserAsync(user, registrationModel, role);
            if (result.Succeeded)
            {
                if (user is Customer customer)
                {
                    await _cartService.CreateCustomerCart(customer);
                }
                return RedirectToAction(role, "Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(registrationModel);
        }

    }

}
