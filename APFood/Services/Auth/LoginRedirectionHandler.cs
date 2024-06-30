using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace APFood.Services.Auth
{
    public class LoginRedirectionHandler : CookieAuthenticationEvents
    {
        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            var requestPath = context.Request.Path;

            if (requestPath.StartsWithSegments("/order", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWithSegments("/cart", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWithSegments("/customer", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWithSegments("/DeliveryTask", StringComparison.OrdinalIgnoreCase) ||
                requestPath.StartsWithSegments("/Payment", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/login/customer");
            }
            else if (requestPath.StartsWithSegments("/FoodVendor", StringComparison.OrdinalIgnoreCase) ||
                     requestPath.StartsWithSegments("/Food", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Redirect("/login/foodVendor");
            }
            else
            {
                context.Response.Redirect("/");
            }

            return Task.CompletedTask;
        }
    }
}
