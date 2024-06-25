using APFood.Constants;
using Microsoft.AspNetCore.Identity;

namespace APFood.Areas.Identity.Data
{
    public class DbSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<APFoodUser>>();

            await SeedRolesAsync(roleManager);
            await SeedSuperAdminAsync(userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = [UserRole.Customer, UserRole.FoodVendor, UserRole.Admin];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<APFoodUser> userManager)
        {
            string email = "admin.apfood@apu.edu.my";
            string password = "Admin@123";

            var superadmin = await userManager.FindByEmailAsync(email);
            if (superadmin == null)
            {
                superadmin = new APFoodUser { UserName = email, Email = email };
                var createResult = await userManager.CreateAsync(superadmin, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(superadmin, UserRole.Admin);
                }
            }
        }
    }
}
