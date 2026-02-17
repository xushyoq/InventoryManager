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
        return View();
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponce") };

        return Challenge(properties, provider);
    }

    public async Task<IActionResult> GoogleResponce()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

        if (result?.Principal == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var imageUrl = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        var user = _context.Users.FirstOrDefault(u => u.Provider == "Google" && u.ProviderUserId == googleId);

        if (user == null)
        {
            user = new Models.User
            {
                Email = email,
                Name = name,
                ProfileImageUrl = imageUrl,
                Provider = "Google",
                ProviderUserId = googleId,
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
            new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "")
        };

        var claimsIdentity = new ClaimsIdentity(localClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }
}