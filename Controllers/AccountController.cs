using System.Security.Claims;
using InventoryManager.Data.Repositories;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

public class AccountController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryAccessRepository _accessRepository;
    private readonly IConfiguration _configuration;

    public AccountController(
        IUserRepository userRepository,
        IInventoryRepository inventoryRepository,
        IInventoryAccessRepository accessRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
        _accessRepository = accessRepository;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(HomeController.Index), "Home");

        return View();
    }

    [HttpPost]
    public IActionResult ExternalLogin(string provider)
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("ExternalLoginCallback") };
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> ExternalLoginCallback(CancellationToken ct)
    {
        var result = await HttpContext.AuthenticateAsync("External");
        if (result?.Principal == null)
            return RedirectToAction(nameof(Login));

        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var providerId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(providerId))
            return RedirectToAction(nameof(Login));

        var imageUrl = claims.FirstOrDefault(c => c.Type == "picture")?.Value
            ?? claims.FirstOrDefault(c => c.Type == "urn:github:avatar")?.Value;
        var providerName = result.Properties?.Items[".AuthScheme"]
            ?? result.Ticket?.AuthenticationScheme
            ?? "Unknown";

        var user = await _userRepository.GetByProviderAsync(providerName, providerId, ct);

        if (user == null)
        {
            user = new User
            {
                Email = email ?? $"{providerId}@auth.com",
                Name = name ?? "NewUser",
                ProfileImageUrl = imageUrl,
                Provider = providerName,
                ProviderUserId = providerId,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user, ct);
        }
        else if (user.IsBlocked)
        {
            await HttpContext.SignOutAsync("External");
            return RedirectToAction(nameof(Blocked));
        }

        if (imageUrl != null && imageUrl != user.ProfileImageUrl)
        {
            user.ProfileImageUrl = imageUrl;
            await _userRepository.UpdateAsync(user, ct);
        }

        var adminEmail = _configuration["AdminEmail"];
        if (!string.IsNullOrEmpty(adminEmail) &&
            string.Equals(user.Email, adminEmail, StringComparison.OrdinalIgnoreCase) &&
            !user.IsAdmin)
        {
            user.IsAdmin = true;
            await _userRepository.UpdateAsync(user, ct);
        }

        var localClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("IsAdmin", user.IsAdmin.ToString()),
            new("ProfileImageUrl", user.ProfileImageUrl ?? "")
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
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.FindFirstValue("IsAdmin") == "True";

        var myInventories = await _inventoryRepository.GetByCreatorAsync(userId, ct);

        List<Inventory> inventoriesWithAccess;
        IEnumerable<Inventory>? allInventories = null;

        if (isAdmin)
        {
            allInventories = await _inventoryRepository.GetAllWithTagsAsync(ct);
            inventoriesWithAccess = [];
        }
        else
        {
            inventoriesWithAccess = (await _inventoryRepository.GetByUserAccessAsync(userId, ct)).ToList();
        }

        ViewBag.MyInventories = myInventories;
        ViewBag.InventoriesWithAccess = inventoriesWithAccess;
        ViewBag.AllInventories = allInventories;
        ViewBag.IsAdmin = isAdmin;

        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUserSuggestions(string? q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var users = await _userRepository.SearchAsync(q, 15, ct);
        return Json(users.Select(u => new { id = u.Id, name = u.Name, email = u.Email ?? "" }));
    }

    [HttpGet]
    public IActionResult Blocked() => View();

    [AllowAnonymous]
    [HttpGet]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (string.IsNullOrEmpty(culture) || (culture != "ru" && culture != "en"))
            culture = "ru";

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
            theme = "light";

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
