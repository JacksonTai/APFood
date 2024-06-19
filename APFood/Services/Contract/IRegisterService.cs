using APFood.Areas.Identity.Data;
using APFood.Models.Register;
using Microsoft.AspNetCore.Identity;

namespace APFood.Services.Contract
{
    public interface IRegisterService
    {
        Task<IdentityResult> RegisterUserAsync(APFoodUser user, RegistrationModel registrationModel, string role);
    }
}
