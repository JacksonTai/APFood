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
        public async Task<IActionResult> Customer(CustomerRegistrationModel customerRegistrationModel)
        {
            return await RegisterUser(new Customer { FullName = customerRegistrationModel.FullName }, 
                customerRegistrationModel, "Customer");
        }

        [HttpPost]
        public async Task<IActionResult> Runner(RegistrationModel registrationModel)
        {
            return await RegisterUser(new Runner { Points = 0 }, registrationModel, "Runner");
        }

        [HttpPost]
        public async Task<IActionResult> FoodVendor(FoodVendorRegistrationModel foodVendorRegistrationModel)
        {
            
            return await RegisterUser(new FoodVendor { StoreName = foodVendorRegistrationModel.StoreName }, 
                foodVendorRegistrationModel, "Food Vendor");
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

        private async Task<IActionResult> RegisterUser(APFoodUser aPFoodUser, RegistrationModel registrationModel, string role)
        {
            if (registrationModel is null)
            {
                throw new ArgumentNullException(nameof(APFoodUser));
            }
            if (!ModelState.IsValid)
            {
                return View(registrationModel);
            }
 
            var result = await TryRegisterUser(aPFoodUser, registrationModel, role);
            if (result.Succeeded)
            {
                var customer = aPFoodUser as Customer;
                if (customer != null)
                {
                    await CreateCustomerCart(customer);
                }
                else
                {
                    throw new InvalidCastException("Unable to cast APFoodUser to Customer.");
                }
                return RedirectToAction(role.Replace(" ", ""), "Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(registrationModel);
        }

        private async Task<IdentityResult> TryRegisterUser(APFoodUser aPFoodUser, RegistrationModel registrationModel, string role)
        {
            APFoodUser? user = await _userManager.FindByEmailAsync(registrationModel.Email);
            if (user != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already in use." });
            }
            await _userStore.SetUserNameAsync(aPFoodUser, registrationModel.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(aPFoodUser, registrationModel.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(aPFoodUser, registrationModel.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Specified role does not exist." });
            }

            var roleResult = await _userManager.AddToRoleAsync(aPFoodUser, role);
            if (!roleResult.Succeeded)
            {
                return roleResult;
            }

            return IdentityResult.Success;
        }

        private async Task CreateCustomerCart(Customer customer)
        {
            if (customer != null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var cart = new Cart
                        {
                            CustomerId = customer.Id,
                            Customer = customer,
                            Items = []
                        };
                        _context.Carts.Add(cart);
                        await _context.SaveChangesAsync();

                        customer.CartId = cart.Id;
                        customer.Cart = cart;
                        _context.Customers.Update(customer);
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(customer));
            }
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
