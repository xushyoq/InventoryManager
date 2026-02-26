using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        inventory.CreatedAt = DateTime.UtcNow;

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }




}