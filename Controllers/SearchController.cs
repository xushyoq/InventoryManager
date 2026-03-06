using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers;

public class SearchController : Controller
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new SearchResult());
        }

        var term = q.Trim().ToLowerInvariant();

        var visibleInventories = _context.Inventories.AsQueryable();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            visibleInventories = visibleInventories.Where(i => i.IsPublic || i.CreatedById == userId);

        }
        else
        {
            visibleInventories = visibleInventories.Where(i => i.IsPublic);
        }

        var inventories = await visibleInventories
            .Include(i => i.InventoryTags)
            .ThenInclude(it => it.Tag)
            .Where(i => (i.Name != null && i.Name.ToLower().Contains(term)) ||
                        (i.Description != null && i.Description.ToLower().Contains(term)))
            .Take(20)
            .ToListAsync();

        var visibleInventoryIds = visibleInventories.Select(i => i.Id);

        var items = await _context.Items
            .Include(i => i.Inventory)
            .Where(i => visibleInventoryIds.Contains(i.InventoryId) &&
                        ((i.CustomId != null && i.CustomId.ToLower().Contains(term)) ||
                        (i.CustomString1 != null && i.CustomString1.ToLower().Contains(term)) ||
                        (i.CustomString2 != null && i.CustomString2.ToLower().Contains(term)) ||
                        (i.CustomString3 != null && i.CustomString3.ToLower().Contains(term))))
            .Take(50)
            .ToListAsync();

        return View(new SearchResult
        {
            Query = q,
            Inventories = inventories,
            Items = items
        });
    }
}