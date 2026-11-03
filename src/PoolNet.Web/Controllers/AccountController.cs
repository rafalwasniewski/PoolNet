using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PoolNet.Web.Models;

namespace PoolNet.Web.Controllers;

public class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        IConfiguration configuration,
        IPasswordHasher<string> passwordHasher,
        ILogger<AccountController> logger)
    {
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string expectedUser = _configuration["AdminCredentials:Username"] ?? "admin";
        string? passwordHash = _configuration["AdminCredentials:PasswordHash"];

        bool credentialsValid = false;

        if (!string.IsNullOrWhiteSpace(passwordHash))
        {
            PasswordVerificationResult verification = _passwordHasher.VerifyHashedPassword(
                expectedUser,
                passwordHash,
                model.Password);

            credentialsValid = model.Username == expectedUser
                && verification != PasswordVerificationResult.Failed;
        }
        else
        {
            _logger.LogWarning(
                "AdminCredentials:PasswordHash is not configured. Administrator login is disabled.");
        }

        if (credentialsValid)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "Administrator"),
                new(ClaimTypes.Role, "Admin")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            TempData["SuccessMessage"] = "Zalogowano pomyślnie jako Administrator.";

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Nieprawidłowy login lub hasło administratora.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Wylogowano z konta administratora.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
