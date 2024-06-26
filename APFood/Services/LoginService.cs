using APFood.Areas.Identity.Data;
using APFood.Services.Contract;
using Microsoft.AspNetCore.Identity;

namespace APFood.Services
{
    public class LoginService(
        SignInManager<APFoodUser> signInManager,
        UserManager<APFoodUser> userManager) : ILoginService
    {
        private readonly SignInManager<APFoodUser> _signInManager = signInManager;
        private readonly UserManager<APFoodUser> _userManager = userManager;

        public async Task<SignInResult> LoginUserAsync(string email, string password, string role)
        {
            APFoodUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return SignInResult.Failed;
            }

            // Check if the user has the expected role
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return await _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
            }

            return SignInResult.Failed;
        }
    }
}
