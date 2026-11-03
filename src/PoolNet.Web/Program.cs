using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using PoolNet.Application;
using PoolNet.Infrastructure;
using PoolNet.Infrastructure.Data;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=poolnet.db";

builder.Services.AddInfrastructureServices(connectionString);
builder.Services.AddApplicationServices();

builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = 5;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
    });

var app = builder.Build();

// Inicjalizacja bazy danych SQLite. Dane demonstracyjne są opcjonalne i nigdy nie nadpisują istniejących danych.
if (app.Configuration.GetValue<bool>("SeedData:Enabled"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Przekierowanie na HTTPS ma sens tylko wtedy, gdy aplikacja rzeczywiście nasłuchuje na HTTPS.
// W kontenerze TLS jest zwykle terminowany na reverse proxy.
var serverAddresses = app.Services.GetService<IServer>()
    ?.Features.Get<IServerAddressesFeature>()?.Addresses;
if (serverAddresses?.Any(address => address.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) == true)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
