using APFood.Areas.Identity.Data;
using APFood.Constants;
using APFood.Models;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class LoginController(ILoginService loginService) : Controller
    {
        private readonly ILoginService _loginService = loginService;

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

        [HttpPost]
        public async Task<IActionResult> Customer(LoginModel loginModel)
        {
            return await LoginUser(loginModel, UserRole.Customer);
        }

        [HttpPost]
        public async Task<IActionResult> FoodVendor(LoginModel loginModel)
        {
            return await LoginUser(loginModel, UserRole.FoodVendor);
        }

        [HttpPost]
        public async Task<IActionResult> Admin(LoginModel loginModel)
        {
            return await LoginUser(loginModel, UserRole.Admin);
        }

        private async Task<IActionResult> LoginUser(LoginModel loginModel, string role)
        {
            var result = await _loginService.LoginUserAsync(loginModel.Email, loginModel.Password, role);
            if (result.Succeeded)
            {
                switch (role)
                {
                    case UserRole.Customer:
                        return RedirectToAction("Index", "Customer");
                    case UserRole.FoodVendor:
                        return RedirectToAction("Index", "FoodVendor");
                    case UserRole.Admin:
                        return RedirectToAction("Index", "Admin");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View();
        }

    }
}
