using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InventoryManager.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly AppDbContext _context;

    public InventoryController(AppDbContext context)
    {
        _context = context;
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

        _context.Inventories.Update(inventory);

        inventory.Version++;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "Данные были изменены другим пользователем. Обновите страницу и попробуйте снова.");
            ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Electronics", "Clothing" });
            return View(inventory);
        }

        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    public async Task<IActionResult> Delete(int[] inventoryIds)
    {
        var inventories = _context.Inventories.Where(i => inventoryIds.Contains(i.Id));

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

        var items = await _context.Items
            .Where(i => i.InventoryId == inventoryId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        ViewBag.Inventory = inventory;

        return View(items);
    }

    private bool CanEditInventory(Inventory inventory)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        var isAdmin = Convert.ToBoolean(User.FindFirstValue("IsAdmin"));

        return inventory.CreatedById == userId || isAdmin;
    }

}