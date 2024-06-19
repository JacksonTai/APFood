using Microsoft.AspNetCore.Identity;

namespace APFood.Services.Contract
{
    public interface ILoginService
    {
        Task<SignInResult> LoginUserAsync(string email, string password, string role);
    }
}
