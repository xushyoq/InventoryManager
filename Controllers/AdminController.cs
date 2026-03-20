using System.Security.Claims;
using InventoryManager.Data.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();
        var users = await _userRepository.GetAllAsync(ct);
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Block(int[] userIds, CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();

        var users = (await _userRepository.GetByIdsAsync(userIds, ct)).ToList();
        foreach (var user in users)
            user.IsBlocked = true;

        foreach (var user in users)
            await _userRepository.UpdateAsync(user, ct);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Unblock(int[] userIds, CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();

        var users = (await _userRepository.GetByIdsAsync(userIds, ct)).ToList();
        foreach (var user in users)
            user.IsBlocked = false;

        foreach (var user in users)
            await _userRepository.UpdateAsync(user, ct);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] userIds, CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();

        var users = (await _userRepository.GetByIdsAsync(userIds, ct)).ToList();
        await _userRepository.RemoveRangeAsync(users, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MakeAdmin(int[] userIds, CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();

        var users = (await _userRepository.GetByIdsAsync(userIds, ct)).ToList();
        foreach (var user in users)
            user.IsAdmin = true;

        foreach (var user in users)
            await _userRepository.UpdateAsync(user, ct);

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userIds != null && userIds.Contains(currentUserId))
            await RefreshCurrentUserClaimsAsync(currentUserId, ct);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveAdmin(int[] userIds, CancellationToken ct)
    {
        if (!IsAdmin()) return Forbid();

        var users = (await _userRepository.GetByIdsAsync(userIds, ct)).ToList();
        foreach (var user in users)
            user.IsAdmin = false;

        foreach (var user in users)
            await _userRepository.UpdateAsync(user, ct);

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (userIds != null && userIds.Contains(currentUserId))
            await RefreshCurrentUserClaimsAsync(currentUserId, ct);

        return RedirectToAction(nameof(Index));
    }

    private bool IsAdmin() => User.FindFirstValue("IsAdmin") == "True";

    private async Task RefreshCurrentUserClaimsAsync(int userId, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(userId, ct);
        if (user == null) return;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new("IsAdmin", user.IsAdmin.ToString()),
            new("ProfileImageUrl", user.ProfileImageUrl ?? "")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
