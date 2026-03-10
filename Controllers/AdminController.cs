using System.Security.Claims;
using InventoryManager.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .OrderBy(u => u.Name)
            .ToListAsync();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Block(int[] userIds)
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.IsBlocked = true;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(int[] userIds)
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.IsBlocked = false;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] userIds)
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MakeAdmin(int[] userIds)
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.IsAdmin = true;
        }

        await _context.SaveChangesAsync();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (userIds != null && userIds.Contains(currentUserId))
        {
            await RefreshCurrentUserClaimsAsync(currentUserId);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveAdmin(int[] userIds)
    {
        if (User.FindFirstValue("IsAdmin") != "True")
        {
            return Forbid();
        }

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in users)
        {
            user.IsAdmin = false;
        }

        await _context.SaveChangesAsync();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (userIds != null && userIds.Contains(currentUserId))
        {
            await RefreshCurrentUserClaimsAsync(currentUserId);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task RefreshCurrentUserClaimsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return;
        }

        var localClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("IsAdmin", user.IsAdmin.ToString()),
            new Claim("ProfileImageUrl", user.ProfileImageUrl ?? "")
        };

        var identity = new ClaimsIdentity(localClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}