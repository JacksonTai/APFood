using APFood.Areas.Identity.Data;
using APFood.Models.Register;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Identity;
namespace APFood.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<APFoodUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<APFoodUser> _userStore;
        private readonly IUserEmailStore<APFoodUser> _emailStore;

        public RegisterService(
            UserManager<APFoodUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<APFoodUser> userStore
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
        }

        public async Task<IdentityResult> RegisterUserAsync(APFoodUser user, RegistrationModel registrationModel, string role)
        {
            APFoodUser? existingUser = await _userManager.FindByEmailAsync(registrationModel.Email);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email already in use." });
            }

            await _userStore.SetUserNameAsync(user, registrationModel.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, registrationModel.Email, CancellationToken.None);
            IdentityResult result = await _userManager.CreateAsync(user, registrationModel.Password);
            if (!result.Succeeded)
            {
                return result;
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Specified role does not exist." });
            }

            IdentityResult roleResult = await _userManager.AddToRoleAsync(user, role);
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
