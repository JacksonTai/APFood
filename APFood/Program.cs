using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using APFood.Data;
using APFood.Areas.Identity.Data;
using APFood.Services;

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

builder.Services.AddScoped<APFood.Services.Contract.IRegisterService, RegisterService>();
builder.Services.AddScoped<APFood.Services.Contract.ILoginService, LoginService>();
builder.Services.AddScoped<APFood.Services.Contract.IRunnerPointService, RunnerPointService>();
builder.Services.AddScoped<APFood.Services.Contract.ICartService, CartService>();
builder.Services.AddScoped<APFood.Services.Contract.IPaymentService, PaymentService>();
builder.Services.AddScoped<APFood.Services.Contract.IOrderService, OrderService>();
builder.Services.AddScoped<APFood.Services.Contract.IDeliveryTaskService, DeliveryTaskService>();
builder.Services.AddScoped<SessionManager>();

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

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.Initialize(scope.ServiceProvider);
}

app.Run();
