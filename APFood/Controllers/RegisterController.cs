using APFood.Areas.Identity.Data;
using APFood.Data;
using APFood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace APFood.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<APFoodUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<APFoodUser> _userStore;
        private readonly IUserEmailStore<APFoodUser> _emailStore;
        private readonly SignInManager<APFoodUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly APFoodContext _context;

        public RegisterController(
            UserManager<APFoodUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<APFoodUser> userStore,
            SignInManager<APFoodUser> signInManager,
            IConfiguration configuration,
            APFoodContext context
          )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Customer(RegistrationModel registrationModel)
        {
            return await RegisterUser(registrationModel, "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> Runner(RegistrationModel registrationModel)
        {
            return await RegisterUser(registrationModel, "Runner");
        }

        [HttpPost]
        public async Task<IActionResult> FoodVendor(FoodVendorRegistrationModel foodVendorRegistrationModel)
        {
            return await RegisterUser(foodVendorRegistrationModel, "Food Vendor");
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

        private async Task<IActionResult> RegisterUser(RegistrationModel registrationModel, String role)
        {
            if (!ModelState.IsValid)
            {
                return View(registrationModel);
            }

            var result = await TryRegisterUser(registrationModel, role);
            if (result.Succeeded)
            {
                return RedirectToAction(role.Replace(" ", ""), "Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registrationModel);
        }

        private async Task<IdentityResult> TryRegisterUser(RegistrationModel registrationModel, string role)
        {
            var userExist = await _userManager.FindByEmailAsync(registrationModel.Email);
            if (userExist != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already in use." });
            }

            APFoodUser user;
            if (role == "Food Vendor" && registrationModel is FoodVendorRegistrationModel foodVendorModel)
            {
                user = new FoodVendor
                {
                    storeName = foodVendorModel.storeName
                };
            }
            else
            {
                return IdentityResult.Failed(new IdentityError { Description = "Invalid user role specified." });
            }

            await _userStore.SetUserNameAsync(user, registrationModel.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, registrationModel.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, registrationModel.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Specified role does not exist." });
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return IdentityResult.Success;
        }


        private IUserEmailStore<APFoodUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<APFoodUser>)_userStore;
        }
    }

}
