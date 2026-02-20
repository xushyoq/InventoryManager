using System.Security.Claims;
using InventoryManager.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        return View();
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback") };

        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback()
    {
        var result = await HttpContext.AuthenticateAsync("External");

        if (result?.Principal == null)
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(providerId))
        {
            return RedirectToAction("Login", "Account");
        }
        var imageUrl = claims.FirstOrDefault(c => c.Type == "picture")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "urn:github:avatar")?.Value;
        var providerName = result.Ticket?.AuthenticationScheme
                        ?? result.Properties?.Items[".AuthScheme"]
                        ?? "Unknown";

        var user = _context.Users.FirstOrDefault(u => u.Provider == providerName && u.ProviderUserId == providerId);

        if (user == null)
        {
            user = new Models.User
            {
                Email = email ?? $"{providerId}@auth.com",
                Name = name ?? "NewUser",
                ProfileImageUrl = imageUrl,
                Provider = providerName,
                ProviderUserId = providerId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var localClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var claimsIdentity = new ClaimsIdentity(localClaims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties { IsPersistent = true });

        await HttpContext.SignOutAsync("External");

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }
}