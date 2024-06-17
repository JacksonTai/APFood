using APFood.Constants;
using APFood.Data;
using Microsoft.AspNetCore.Identity;

namespace APFood.Areas.Identity.Data
{
    public class DbSeeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<APFoodUser>>();
            var context = serviceProvider.GetRequiredService<APFoodContext>();

            await SeedRolesAsync(roleManager);
            await SeedSuperAdminAsync(userManager);
            SeedFoodData(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { UserRole.Customer, UserRole.FoodVendor, UserRole.Admin };
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

        private static void SeedFoodData(APFoodContext context)
        {
            if (!context.Foods.Any())
            {
                context.Foods.AddRange(
                    new Food { Name = "Pizza", Price = 8.99m },
                    new Food { Name = "Burger", Price = 5.49m },
                    new Food { Name = "Salad", Price = 4.75m },
                    new Food { Name = "Pasta", Price = 7.99m }
                );
                context.SaveChanges();
            }
        }
    }
}
