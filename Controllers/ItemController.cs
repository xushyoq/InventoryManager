using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers;

[Authorize]
public class ItemController : Controller
{
    private readonly AppDbContext _context;

    public ItemController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
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

        ViewBag.Inventory = inventory;

        return View(inventoryId);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Item item, int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (!ModelState.IsValid)
        {
            return View(inventory);
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
        if (!ModelState.IsValid)
        {
            return View(item);
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

        item.CreatedAt = DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc);

        _context.Update(item);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { inventoryId = inventory.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] itemIds)
    {
        var items = _context.Items.Where(i => itemIds.Contains(i.Id));

        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == items.First().InventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditInventory(inventory))
        {
            return Forbid();
        }

        _context.RemoveRange(items);
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