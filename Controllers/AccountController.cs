using System.Security.Claims;
using InventoryManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
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
            return RedirectToAction("Login", "Account");
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
        var providerName = result.Properties?.Items[".AuthScheme"]
            ?? result.Ticket?.AuthenticationScheme
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
        else if (user.IsBlocked == true)
        {
            await HttpContext.SignOutAsync("External");

            return RedirectToAction("Blocked");
        }

        if (imageUrl != null && imageUrl != user.ProfileImageUrl)
        {
            user.ProfileImageUrl = imageUrl;
            await _context.SaveChangesAsync();
        }

        var localClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("IsAdmin", user.IsAdmin.ToString()),
            new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "")
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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);

        var myInventories = await _context.Inventories
            .Where(i => i.CreatedById == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var invIdsWithAccess = await _context.InventoryAccesses
            .Where(a => a.UserId == userId)
            .Select(a => a.InventoryId)
            .ToListAsync();
        var inventoriesWithAccess = await _context.Inventories
            .Include(i => i.CreatedBy)
            .Where(i => invIdsWithAccess.Contains(i.Id))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        ViewBag.MyInventories = myInventories;
        ViewBag.InventoriesWithAccess = inventoriesWithAccess;

        return View();
    }

    [HttpGet]
    public IActionResult Blocked()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (string.IsNullOrEmpty(culture) || (culture != "ru" && culture != "en"))
        {
            culture = "ru";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") ? returnUrl : "/";
        return LocalRedirect(redirectUrl);
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult SetTheme(string theme, string? returnUrl)
    {
        if (string.IsNullOrEmpty(theme) || (theme != "light" && theme != "dark"))
        {
            theme = "light";
        }

        Response.Cookies.Append(
            "theme",
            theme,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/",
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        var redirectUrl = !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith("/") ? returnUrl : "/";
        return LocalRedirect(redirectUrl);
    }
}