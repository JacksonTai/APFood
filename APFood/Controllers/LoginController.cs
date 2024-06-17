using APFood.Areas.Identity.Data;
using APFood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace APFood.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<APFoodUser> _signInManager;
        private readonly UserManager<APFoodUser> _userManager;

        public LoginController(SignInManager<APFoodUser> signInManager, UserManager<APFoodUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Customer()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Runner()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FoodVendor()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Customer(LoginModel loginModel)
        {
            return await LoginUser(loginModel, "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> Runner(LoginModel loginModel)
        {
            return await LoginUser(loginModel, "Runner");
        }

        [HttpPost]
        public async Task<IActionResult> FoodVendor(LoginModel loginModel)
        {
            return await LoginUser(loginModel, "Food Vendor");
        }

        private async Task<IActionResult> LoginUser(LoginModel loginModel, string role)
        {
            APFoodUser? user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }

            // Check if the user has the expected role
            if (await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View();
        }

    }
}
