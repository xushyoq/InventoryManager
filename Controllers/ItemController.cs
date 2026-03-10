using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using InventoryManager.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

[Authorize]
public class ItemController : Controller
{
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public ItemController(AppDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditInventory(inventory))
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

    [HttpGet]
    public async Task<IActionResult> Create(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditInventory(inventory))
        {
            return Forbid();
        }

        ViewBag.Inventory = inventory;

        return View(inventoryId);
    }

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

        if (!CanEditInventory(inventory))
        {
            return Forbid();
        }

        ViewBag.Inventory = inventory;

        return View(item);

    }

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

        if (!CanEditInventory(inventory))
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

        if (!CanEditInventory(inventory))
        {
            return Forbid();
        }

        _context.RemoveRange(itemsToDelete);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { inventoryId = inventory.Id });
    }

    private bool CanEditInventory(Inventory inventory)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        var isAdmin = Convert.ToBoolean(User.FindFirstValue("IsAdmin"));

        return inventory.CreatedById == userId || isAdmin;
    }


}