using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using APFood.Data;
using APFood.Areas.Identity.Data;
using APFood.Constants.Order;
using APFood.Constants;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APFoodContextConnection") ?? throw new InvalidOperationException("Connection string 'APFoodContextConnection' not found.");

builder.Services.AddDbContext<APFoodContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<APFoodUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<APFoodContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<APFood.Services.Contract.IRegisterService, APFood.Services.RegisterService>();
builder.Services.AddScoped<APFood.Services.Contract.ILoginService, APFood.Services.LoginService>();
builder.Services.AddScoped<APFood.Services.Contract.ICartService, APFood.Services.CartService>();
builder.Services.AddScoped<APFood.Services.Contract.IPaymentService, APFood.Services.PaymentService>();
builder.Services.AddScoped<APFood.Services.Contract.IOrderService, APFood.Services.OrderService>();
builder.Services.AddScoped<APFood.Services.Contract.IDeliveryTaskService, APFood.Services.DeliveryTaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseSession();
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
    string[] roles = [UserRole.Customer, UserRole.FoodVendor, UserRole.Admin];
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

    // Seed single superadmin
    string email = "admin.apfood@apu.edu.my";
    string password = "Admin@123";
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<APFoodUser>>();
    APFoodUser? superadmin = await userManager.FindByEmailAsync(email);
    if (superadmin == null)
    {
        superadmin = new APFoodUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(superadmin, password);
        await userManager.AddToRoleAsync(superadmin, UserRole.Admin);
    }
}

static void SeedFoodData(APFoodContext context)
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
