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
        var queryLower = query.ToLowerInvariant();

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

        var visibleInventoryIds = await visibleInventories.Select(i => i.Id).ToListAsync();

        var inventoriesByFts = await visibleInventories
            .Include(i => i.InventoryTags)
            .ThenInclude(it => it.Tag)
            .Where(i => i.SearchVector != null && i.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", query)))
            .Take(20)
            .ToListAsync();

        var itemsByFts = await _context.Items
            .Include(i => i.Inventory)
            .Where(i => visibleInventoryIds.Contains(i.InventoryId) &&
                        i.SearchVector != null &&
                        i.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", query)))
            .Take(50)
            .ToListAsync();

        var tagMatchingInventoryIds = await _context.InventoryTags
            .Where(it => it.Tag != null && it.Tag.Name.ToLower().Contains(queryLower))
            .Select(it => it.InventoryId)
            .Where(id => visibleInventoryIds.Contains(id))
            .Distinct()
            .ToListAsync();

        var inventoriesByTag = tagMatchingInventoryIds.Count > 0
            ? await visibleInventories
                .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
                .Where(i => tagMatchingInventoryIds.Contains(i.Id))
                .ToListAsync()
            : new List<Inventory>();

        var itemsByTag = tagMatchingInventoryIds.Count > 0
            ? await _context.Items
                .Include(i => i.Inventory)
                .Where(i => tagMatchingInventoryIds.Contains(i.InventoryId))
                .Take(50)
                .ToListAsync()
            : new List<Item>();

        // Объединяем результаты (без дубликатов)
        var inventories = inventoriesByFts
            .UnionBy(inventoriesByTag, i => i.Id)
            .Take(20)
            .ToList();

        var items = itemsByFts
            .UnionBy(itemsByTag, i => i.Id)
            .Take(50)
            .ToList();

        return View(new SearchResult
        {
            Query = q,
            Inventories = inventories,
            Items = items
        });
    }
}