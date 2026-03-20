using System.Security.Claims;
using InventoryManager.Data.Repositories;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IInventoryService _inventoryService;
    private readonly IItemRepository _itemRepository;
    private readonly ILikeRepository _likeRepository;
    private readonly IInventoryCommentRepository _commentRepository;
    private readonly IInventoryAccessRepository _accessRepository;
    private readonly IUserRepository _userRepository;
    private readonly IStringLocalizer<SharedResource> _localizer;

    private static readonly string[] Categories = ["Other", "Books", "Electronics", "Clothing"];

    public InventoryController(
        IInventoryService inventoryService,
        IItemRepository itemRepository,
        ILikeRepository likeRepository,
        IInventoryCommentRepository commentRepository,
        IInventoryAccessRepository accessRepository,
        IUserRepository userRepository,
        IStringLocalizer<SharedResource> localizer)
    {
        _inventoryService = inventoryService;
        _itemRepository = itemRepository;
        _likeRepository = likeRepository;
        _commentRepository = commentRepository;
        _accessRepository = accessRepository;
        _userRepository = userRepository;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var userId = GetUserId();
        var inventories = await _inventoryService.GetByCreatorAsync(userId, ct);
        return View(inventories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(Categories);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Inventory inventory, string? TagsInput, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(Categories);
            return View(inventory);
        }

        var tagNames = ParseTagNames(TagsInput);
        await _inventoryService.CreateAsync(inventory, tagNames, GetUserId(), ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetTagSuggestions(string? q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(Array.Empty<object>());

        var suggestions = await _inventoryService.GetTagSuggestionsAsync(q, ct);
        return Json(suggestions.Select(t => new { id = t.Id, name = t.Name }));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int inventoryId, string? returnUrl, CancellationToken ct)
    {
        ViewBag.ReturnUrl = returnUrl;

        var inventory = await _inventoryService.GetByIdWithTagsAsync(inventoryId, ct);
        if (inventory == null)
            return NotFound();

        if (!CanEditInventory(inventory))
            return RedirectToAction(nameof(Index));

        ViewBag.Categories = new SelectList(Categories);
        return View(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Inventory inventory, string? TagsInput, string? returnUrl, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(Categories);
            ViewBag.ReturnUrl = returnUrl;
            return View(inventory);
        }

        var tagNames = ParseTagNames(TagsInput);

        try
        {
            await _inventoryService.EditAsync(inventory, tagNames, GetUserId(), IsAdmin(), ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", _localizer["ConcurrencyError"].Value);
            ViewBag.Categories = new SelectList(Categories);
            ViewBag.ReturnUrl = returnUrl;
            return View(inventory);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] inventoryIds, string? returnUrl, CancellationToken ct)
    {
        try
        {
            await _inventoryService.DeleteAsync(inventoryIds, GetUserId(), IsAdmin(), ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int inventoryId, CancellationToken ct)
    {
        var inventory = await _inventoryService.GetByIdAsync(inventoryId, ct);
        if (inventory == null)
            return NotFound();

        var items = (await _itemRepository.GetByInventoryIdAsync(inventoryId, ct)).ToList();
        var itemIds = items.Select(i => i.Id).ToList();

        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        IEnumerable<int> likedItemIds = [];
        if (currentUserIdString != null)
        {
            likedItemIds = await _likeRepository.GetLikedItemIdsAsync(int.Parse(currentUserIdString), itemIds, ct);
        }

        var comments = await _commentRepository.GetByInventoryIdAsync(inventoryId, ct);
        var accesses = await _accessRepository.GetByInventoryIdAsync(inventoryId, ct);
        var stats = ComputeStatistics(inventory, items);

        ViewBag.Inventory = inventory;
        ViewBag.Comments = comments;
        ViewBag.Accesses = accesses;
        ViewBag.Statistics = stats;
        ViewBag.CanEditInventory = CanEditInventory(inventory);
        ViewBag.CanEditItems = await CanEditItemsAsync(inventory, ct);
        ViewBag.LikedItemIds = new HashSet<int>(likedItemIds);
        ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;

        return View(items);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int inventoryId, string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 2000)
            return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#discussion");

        if (!await _inventoryService.ExistsAsync(inventoryId, ct))
            return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Challenge();

        await _commentRepository.AddAsync(new InventoryComment
        {
            InventoryId = inventoryId,
            UserId = int.Parse(userIdString),
            Text = text.Trim()
        }, ct);

        return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#discussion");
    }

    [HttpPost]
    public async Task<IActionResult> GrantAccess(int inventoryId, int? userId, string? userEmail, CancellationToken ct)
    {
        var inventory = await _inventoryService.GetByIdAsync(inventoryId, ct);
        if (inventory == null || !CanEditInventory(inventory))
            return Forbid();

        User? user = null;
        if (userId.HasValue)
            user = await _userRepository.GetByIdAsync(userId.Value, ct);
        else if (!string.IsNullOrWhiteSpace(userEmail))
            user = await _userRepository.GetByEmailAsync(userEmail.Trim(), ct);

        if (user == null)
        {
            TempData["AccessError"] = _localizer["UserNotFound"].Value;
            return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#access");
        }

        if (user.Id == inventory.CreatedById)
        {
            TempData["AccessError"] = _localizer["OwnerHasFullAccess"].Value;
            return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#access");
        }

        if (!await _accessRepository.ExistsAsync(inventoryId, user.Id, ct))
        {
            await _accessRepository.AddAsync(new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = user.Id
            }, ct);
        }

        return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#access");
    }

    [HttpPost]
    public async Task<IActionResult> RevokeAccess(int inventoryId, int userId, CancellationToken ct)
    {
        var inventory = await _inventoryService.GetByIdAsync(inventoryId, ct);
        if (inventory == null || !CanEditInventory(inventory))
            return Forbid();

        await _accessRepository.RevokeAccessAsync(inventoryId, userId, ct);

        return Redirect(Url.Action(nameof(Details), new { inventoryId }) + "#access");
    }

    private static IReadOnlyList<string> ParseTagNames(string? tagsInput) =>
        (tagsInput ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.FindFirstValue("IsAdmin") == "True";

    private bool CanEditInventory(Inventory inventory)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return false;
        return inventory.CreatedById == int.Parse(userIdString) || IsAdmin();
    }

    private async Task<bool> CanEditItemsAsync(Inventory inventory, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return false;
        var userId = int.Parse(userIdString);

        // Public inventory: any authenticated user can manage items
        if (inventory.IsPublic) return true;

        // Private inventory: owner, admin, or explicitly granted access
        if (inventory.CreatedById == userId || IsAdmin()) return true;
        return await _accessRepository.ExistsAsync(inventory.Id, userId, ct);
    }

    private static InventoryStatistics ComputeStatistics(Inventory inv, List<Item> items)
    {
        var stats = new InventoryStatistics { ItemsCount = items.Count };

        if (inv.CustomInt1State && items.Count > 0)
        {
            var vals = items.Select(i => i.CustomInt1).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Count > 0)
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt1Name ?? "Int1", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomInt2State && items.Count > 0)
        {
            var vals = items.Select(i => i.CustomInt2).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Count > 0)
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt2Name ?? "Int2", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomInt3State && items.Count > 0)
        {
            var vals = items.Select(i => i.CustomInt3).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Count > 0)
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt3Name ?? "Int3", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomString1State && items.Count > 0)
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString1))
                .GroupBy(i => i.CustomString1!).OrderByDescending(g => g.Count()).Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() }).ToList();
            if (top.Count > 0)
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString1Name ?? "String1", Top = top });
        }
        if (inv.CustomString2State && items.Count > 0)
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString2))
                .GroupBy(i => i.CustomString2!).OrderByDescending(g => g.Count()).Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() }).ToList();
            if (top.Count > 0)
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString2Name ?? "String2", Top = top });
        }
        if (inv.CustomString3State && items.Count > 0)
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString3))
                .GroupBy(i => i.CustomString3!).OrderByDescending(g => g.Count()).Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() }).ToList();
            if (top.Count > 0)
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString3Name ?? "String3", Top = top });
        }

        return stats;
    }
}
