using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly AppDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public InventoryController(AppDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        var inventories = await _context.Inventories
            .Where(i => i.CreatedById == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(inventories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Inventory inventory, string? TagsInput)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });
            return View(inventory);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        inventory.CreatedById = userId;
        inventory.CreatedAt = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        var tagNames = (TagsInput ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        foreach (var name in tagNames)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name);

            if (tag == null)
            {
                tag = new Tag { Name = name };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            _context.InventoryTags.Add(new InventoryTag
            {
                InventoryId = inventory.Id,
                TagId = tag.Id
            });
        }

        await _context.SaveChangesAsync();


        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int inventoryId)
    {

        var inventory = _context.Inventories
            .Include(i => i.InventoryTags)
            .ThenInclude(it => it.Tag)
            .FirstOrDefault(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!CanEditInventory(inventory))
        {
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });

        return View(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Inventory inventory, string? TagsInput)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });
            return View(inventory);
        }

        var existingInventory = await _context.Inventories
        .Include(i => i.InventoryTags)
        .FirstOrDefaultAsync(i => i.Id == inventory.Id);

        if (existingInventory == null)
        {
            return NotFound();
        }

        if (!CanEditInventory(existingInventory))
        {
            return Forbid();
        }

        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.CreatedAt = DateTime.SpecifyKind(inventory.CreatedAt, DateTimeKind.Utc);

        var tagNames = (TagsInput ?? "")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(s => s.Trim().ToLowerInvariant())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .Distinct()
        .ToList();

        _context.InventoryTags.RemoveRange(existingInventory.InventoryTags);

        foreach (var name in tagNames)
        {
            var tag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name);

            if (tag == null)
            {
                tag = new Tag { Name = name };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            _context.InventoryTags.Add(new InventoryTag
            {
                InventoryId = inventory.Id,
                TagId = tag.Id
            });
        }

        _context.Entry(existingInventory).CurrentValues.SetValues(inventory);
        existingInventory.UpdatedAt = DateTime.UtcNow;
        existingInventory.Version++;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", _localizer["ConcurrencyError"].Value);
            ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });
            return View(inventory);
        }

        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] inventoryIds)
    {
        var inventories = await _context.Inventories
            .Where(i => inventoryIds.Contains(i.Id))
            .ToListAsync();

        if (inventories.Any(i => !CanEditInventory(i)))
        {
            return Forbid();
        }

        _context.Inventories.RemoveRange(inventories);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        if (!inventory.IsPublic)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Challenge();
            }
            var userId = int.Parse(userIdString);
            var isAdmin = User.FindFirstValue("IsAdmin") == "True";
            var isOwner = inventory.CreatedById == userId;
            var hasAccess = await _context.InventoryAccesses
                .AnyAsync(a => a.InventoryId == inventoryId && a.UserId == userId);

            if (!isOwner && !isAdmin && !hasAccess)
            {
                return Challenge();
            }
        }

        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var comments = await _context.InventoryComments
            .Include(c => c.User)
            .Where(c => c.InventoryId == inventoryId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var accesses = await _context.InventoryAccesses
            .Include(a => a.User)
            .Where(a => a.InventoryId == inventoryId)
            .OrderBy(a => a.GrantedAt)
            .ToListAsync();

        var itemsList = items.ToList();
        var stats = ComputeStatistics(inventory!, itemsList);

        ViewBag.Inventory = inventory;
        ViewBag.Comments = comments;
        ViewBag.Accesses = accesses;
        ViewBag.Statistics = stats;
        ViewBag.CanEdit = inventory != null && CanEditInventory(inventory);

        return View(items);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddComment(int inventoryId, string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 2000)
        {
            return RedirectToAction(nameof(Details), new { inventoryId });
        }

        var inventory = await _context.Inventories.FindAsync(inventoryId);
        if (inventory == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return Challenge();

        var userId = int.Parse(userIdString);

        _context.InventoryComments.Add(new InventoryComment
        {
            InventoryId = inventoryId,
            UserId = userId,
            Text = text.Trim()
        });
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { inventoryId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> GrantAccess(int inventoryId, string userEmail)
    {
        var inventory = await _context.Inventories.FindAsync(inventoryId);
        if (inventory == null || !CanEditInventory(inventory!)) return Forbid();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail.Trim());
        if (user == null)
        {
            TempData["AccessError"] = _localizer["UserNotFound"].Value;
            return RedirectToAction(nameof(Details), new { inventoryId });
        }

        if (user.Id == inventory.CreatedById)
        {
            TempData["AccessError"] = _localizer["OwnerHasFullAccess"].Value;
            return RedirectToAction(nameof(Details), new { inventoryId });
        }

        if (!await _context.InventoryAccesses.AnyAsync(a => a.InventoryId == inventoryId && a.UserId == user.Id))
        {
            _context.InventoryAccesses.Add(new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { inventoryId });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> RevokeAccess(int inventoryId, int userId)
    {
        var inventory = await _context.Inventories.FindAsync(inventoryId);
        if (inventory == null || !CanEditInventory(inventory!)) return Forbid();

        var access = await _context.InventoryAccesses
            .FirstOrDefaultAsync(a => a.InventoryId == inventoryId && a.UserId == userId);
        if (access != null)
        {
            _context.InventoryAccesses.Remove(access);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { inventoryId });
    }

    private static InventoryStatistics ComputeStatistics(Inventory inv, List<Item> items)
    {
        var stats = new InventoryStatistics { ItemsCount = items.Count };

        if (inv.CustomInt1State && items.Any())
        {
            var vals = items.Select(i => i.CustomInt1).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Any())
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt1Name ?? "Int1", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomInt2State && items.Any())
        {
            var vals = items.Select(i => i.CustomInt2).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Any())
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt2Name ?? "Int2", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomInt3State && items.Any())
        {
            var vals = items.Select(i => i.CustomInt3).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Any())
                stats.NumericStats.Add(new NumericFieldStats { Name = inv.CustomInt3Name ?? "Int3", Avg = vals.Average(), Min = vals.Min(), Max = vals.Max() });
        }
        if (inv.CustomString1State && items.Any())
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString1))
                .GroupBy(i => i.CustomString1!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() })
                .ToList();
            if (top.Any())
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString1Name ?? "String1", Top = top });
        }
        if (inv.CustomString2State && items.Any())
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString2))
                .GroupBy(i => i.CustomString2!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() })
                .ToList();
            if (top.Any())
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString2Name ?? "String2", Top = top });
        }
        if (inv.CustomString3State && items.Any())
        {
            var top = items.Where(i => !string.IsNullOrEmpty(i.CustomString3))
                .GroupBy(i => i.CustomString3!)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new StringCount { Value = g.Key, Count = g.Count() })
                .ToList();
            if (top.Any())
                stats.StringStats.Add(new StringFieldStats { Name = inv.CustomString3Name ?? "String3", Top = top });
        }

        return stats;
    }

    private bool CanEditInventory(Inventory inventory)
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