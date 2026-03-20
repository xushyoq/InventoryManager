using System.Security.Claims;
using InventoryManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

public class ItemController : Controller
{
    private readonly IItemService _itemService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ItemController(IItemService itemService, IStringLocalizer<SharedResource> localizer)
    {
        _itemService = itemService;
        _localizer = localizer;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int itemId, CancellationToken ct)
    {
        var item = await _itemService.GetByIdWithInventoryAndLikesAsync(itemId, ct);
        if (item == null || item.Inventory == null)
            return NotFound();

        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? currentUserId = currentUserIdString != null ? int.Parse(currentUserIdString) : null;

        if (!await _itemService.CanViewInventoryAsync(item.InventoryId, currentUserId, IsAdmin(), ct))
            return Forbid();

        var likedItemIds = new HashSet<int>();
        if (currentUserId.HasValue)
        {
            var liked = item.Likes.Any(l => l.UserId == currentUserId.Value);
            if (liked) likedItemIds.Add(itemId);
        }

        ViewBag.CanEdit = await _itemService.CanEditItemsAsync(item.InventoryId, currentUserId ?? 0, IsAdmin(), ct);
        ViewBag.LikedItemIds = likedItemIds;
        ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;

        return View(item);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(int inventoryId, CancellationToken ct)
    {
        var currentUserId = GetUserId();

        if (!await _itemService.CanEditItemsAsync(inventoryId, currentUserId, IsAdmin(), ct))
            return Forbid();

        var inventory = await _itemService.GetInventoryAsync(inventoryId, ct);
        if (inventory == null)
            return NotFound();

        var items = await _itemService.GetByInventoryIdAsync(inventoryId, ct);

        ViewBag.Inventory = inventory;
        return View(items);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create(int inventoryId, CancellationToken ct)
    {
        var inventory = await _itemService.GetInventoryAsync(inventoryId, ct);
        if (inventory == null)
            return NotFound();

        if (!await _itemService.CanEditItemsAsync(inventoryId, GetUserId(), IsAdmin(), ct))
            return Forbid();

        ViewBag.Inventory = inventory;
        return View(inventoryId);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Models.Item item, int inventoryId, CancellationToken ct)
    {
        var inventory = await _itemService.GetInventoryAsync(inventoryId, ct);

        // If the inventory has a template and the user left CustomId blank,
        // the service will auto-generate it — clear the Required validation error
        if (!string.IsNullOrWhiteSpace(inventory?.CustomIdTemplate)
            && string.IsNullOrWhiteSpace(item.CustomId))
        {
            ModelState.Remove(nameof(item.CustomId));
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Inventory = inventory;
            return View(inventoryId);
        }

        try
        {
            await _itemService.CreateAsync(item, inventoryId, GetUserId(), ct);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Inventory = inventory;
            return View(inventoryId);
        }

        return RedirectToAction(nameof(Index), new { inventoryId });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int itemId, CancellationToken ct)
    {
        var item = await _itemService.GetByIdAsync(itemId, ct);
        if (item == null)
            return NotFound();

        var inventory = await _itemService.GetInventoryAsync(item.InventoryId, ct);
        if (inventory == null)
            return NotFound();

        if (!await _itemService.CanEditItemsAsync(item.InventoryId, GetUserId(), IsAdmin(), ct))
            return Forbid();

        ViewBag.Inventory = inventory;
        return View(item);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(Models.Item item, CancellationToken ct)
    {
        var inventory = await _itemService.GetInventoryAsync(item.InventoryId, ct);

        if (!ModelState.IsValid)
        {
            ViewBag.Inventory = inventory;
            return View(item);
        }

        try
        {
            await _itemService.EditAsync(item, GetUserId(), IsAdmin(), ct);
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
            ViewBag.Inventory = inventory;
            return View(item);
        }

        return RedirectToAction(nameof(Index), new { inventoryId = item.InventoryId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(int[] itemIds, CancellationToken ct)
    {
        if (itemIds == null || itemIds.Length == 0)
            return BadRequest();

        try
        {
            var inventoryId = await _itemService.DeleteAsync(itemIds, GetUserId(), IsAdmin(), ct);
            if (inventoryId == null)
                return NotFound();

            return RedirectToAction(nameof(Index), new { inventoryId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ToggleLike(int itemId, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Challenge();

        try
        {
            var (liked, count) = await _itemService.ToggleLikeAsync(itemId, int.Parse(userIdString), ct);
            return Json(new { liked, count });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.FindFirstValue("IsAdmin") == "True";
}
