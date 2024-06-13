using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using APFood.Data;
using APFood.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APFoodContextConnection") ?? throw new InvalidOperationException("Connection string 'APFoodContextConnection' not found.");

builder.Services.AddDbContext<APFoodContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<APFoodUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<APFoodContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using(var scope = app.Services.CreateScope())
{
    // Roles Seeding
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Food Vendor", "Runner", "Customer" };
    foreach(var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role)); 
        }
    }


    var context = scope.ServiceProvider.GetRequiredService<APFoodContext>();
    context.Database.Migrate();
    SeedFoodData(context);
}

void SeedFoodData(APFoodContext context)
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

app.Run();
