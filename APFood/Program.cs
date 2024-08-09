using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using APFood.Data;
using APFood.Areas.Identity.Data;
using APFood.Services;
using APFood.Services.Auth;
using APFood.Services.Contract;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("APFoodContextConnection") ?? throw new InvalidOperationException("Connection string 'APFoodContextConnection' not found.");

builder.Services.AddDbContext<APFoodContext>(options => {
    options.UseSqlServer(connectionString).EnableSensitiveDataLogging();
});
builder.Services.AddDefaultIdentity<APFoodUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<APFoodContext>();

builder.Services.AddScoped<DbContext, APFoodContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IRunnerPointService, RunnerPointService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IOrderService, OrderService>()
    .AddHttpClient<IOrderService, OrderService>(client =>
    {
        client.BaseAddress = new Uri("https://g68t4gya31.execute-api.us-east-1.amazonaws.com/dev/");
    });

builder.Services.AddScoped<IDeliveryTaskService, DeliveryTaskService>();
builder.Services.AddScoped<SessionManager>();
builder.Services.AddScoped<LoginRedirectionHandler>();
builder.Services.ConfigureApplicationCookie(options =>
{
    // Default path
    options.LoginPath = "/";
    options.EventsType = typeof(LoginRedirectionHandler);
});

// AWS configuration
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<S3Service>();
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

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
