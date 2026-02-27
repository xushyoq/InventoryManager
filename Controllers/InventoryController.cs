using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        ViewBag.Categories = new SelectList(new[] { "Other", "Books", "Elecgtronics", "Clothing" });
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        if (!ModelState.IsValid)
        {
            return View(inventory);
        }

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString);

        inventory.CreatedById = userId;
        inventory.CreatedAt = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int inventoryId)
    {
        var inventory = _context.Inventories.FirstOrDefault(i => i.Id == inventoryId);
        if (inventory == null)
        {
            return NotFound();
        }

        return View(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Inventory inventory)
    {
        if (!ModelState.IsValid)
        {
            return View(inventory);
        }

        inventory.UpdatedAt = DateTime.UtcNow;
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));

    }

    [HttpPost]
    public async Task<IActionResult> Delete(int inventoryId)
    {
        var inventory = _context.Inventories.FirstOrDefault(i => i.Id == inventoryId);

        if (inventory == null)
        {
            return NotFound();
        }

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


}