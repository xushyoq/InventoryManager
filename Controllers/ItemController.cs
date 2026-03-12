using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

public class ItemController : Controller
{
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ItemController(AppDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int itemId)
    {
        var item = await _context.Items
            .Include(i => i.Inventory)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null || item.Inventory == null)
            return NotFound();

        var likedItemIds = new HashSet<int>();
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserIdString != null)
        {
            var liked = await _context.Likes
                .Where(l => l.UserId == int.Parse(currentUserIdString) && l.ItemId == itemId)
                .AnyAsync();
            if (liked) likedItemIds.Add(itemId);
        }

        ViewBag.CanEdit = CanEditItems(item.Inventory);
        ViewBag.LikedItemIds = likedItemIds;
        ViewBag.IsAuthenticated = User.Identity?.IsAuthenticated == true;

        return View(item);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Index(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditItems(inventory))
        {
            return Forbid();
        }

        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        ViewBag.Inventory = inventory;

        return View(items);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Create(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditItems(inventory))
        {
            return Forbid();
        }

        ViewBag.Inventory = inventory;

        return View(inventoryId);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(Item item, int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (!ModelState.IsValid)
        {
            ViewBag.Inventory = inventory;
            return View(inventoryId);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        item.CreatedById = userId;
        item.CreatedAt = DateTime.UtcNow;
        item.InventoryId = inventoryId;

        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { inventoryId = inventoryId });

    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int itemId)
    {
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
        {
            return NotFound();
        }

        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditItems(inventory))
        {
            return Forbid();
        }

        ViewBag.Inventory = inventory;

        return View(item);

    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Edit(Item item)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == item.InventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Inventory = inventory;
            return View(item);
        }

        if (!CanEditItems(inventory))
        {
            return Forbid();
        }

        item.CreatedAt = DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc);

        _context.Update(item);

        item.Version++;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", _localizer["ConcurrencyError"].Value);
            ViewBag.Inventory = inventory;
            return View(item);
        }


        return RedirectToAction(nameof(Index), new { inventoryId = inventory.Id });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Delete(int[] itemIds)
    {

        if (itemIds == null || itemIds.Length == 0)
        {
            return BadRequest();
        }

        var itemsToDelete = await _context.Items.Where(i => itemIds.Contains(i.Id)).ToListAsync();

        if (!itemsToDelete.Any())
        {
            return NotFound();
        }

        var inventoryId = itemsToDelete.First().InventoryId;
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditItems(inventory))
        {
            return Forbid();
        }

        _context.RemoveRange(itemsToDelete);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { inventoryId = inventory.Id });
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ToggleLike(int itemId)
    {
        var item = await _context.Items
            .Include(i => i.Inventory)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null || item.Inventory == null)
            return NotFound();

        if (!CanViewInventory(item.Inventory))
            return Forbid();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Challenge();

        var userId = int.Parse(userIdString);
        var existingLike = item.Likes.FirstOrDefault(l => l.UserId == userId);

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            _context.Likes.Add(new Like { ItemId = itemId, UserId = userId });
        }

        await _context.SaveChangesAsync();

        var count = await _context.Likes.CountAsync(l => l.ItemId == itemId);
        var liked = existingLike == null;

        return Json(new { liked, count });
    }

    private bool CanViewInventory(Inventory inventory)
    {
        if (inventory.IsPublic) return true;

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return false;

        var userId = int.Parse(userIdString);
        if (inventory.CreatedById == userId) return true;
        if (User.FindFirstValue("IsAdmin") == "True") return true;
        return _context.InventoryAccesses.Any(a => a.InventoryId == inventory.Id && a.UserId == userId);
    }

    private bool CanEditItems(Inventory inventory)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return false;

        var userId = int.Parse(userIdString);
        var isAdmin = User.FindFirstValue("IsAdmin") == "True";
        if (inventory.CreatedById == userId || isAdmin) return true;

        return _context.InventoryAccesses
            .Any(a => a.InventoryId == inventory.Id && a.UserId == userId);
    }


}