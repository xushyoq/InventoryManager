using System.Security.Claims;
using InventoryManager.Data;
using InventoryManager.Models;
using Microsoft.AspNetCore.Authorization;
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

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new SearchResult());
        }

        var query = q.Trim();

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
            .Where(i => i.SearchVector!.Matches(EF.Functions.PlainToTsQuery("english", query)))
            .Take(20)
            .ToListAsync();

        var visibleInventoryIds = await visibleInventories.Select(i => i.Id).ToListAsync();

        var items = await _context.Items
            .Include(i => i.Inventory)
            .Where(i => visibleInventoryIds.Contains(i.InventoryId) &&
                        i.SearchVector!.Matches(EF.Functions.PlainToTsQuery("english", query)))
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