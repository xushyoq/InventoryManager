using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventoryManager.Models;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace InventoryManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    private readonly IStringLocalizer<SharedResource> _localizer;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _logger = logger;
        _context = context;
        _localizer = localizer;
    }

    public async Task<IActionResult> Index(string? tag)
    {
        var query = _context.Inventories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagLower = tag.Trim().ToLowerInvariant();
            query = query.Where(i => i.InventoryTags.Any(it => it.Tag != null && it.Tag.Name.ToLower() == tagLower));
        }

        var latestInventories = await query
            .Include(i => i.InventoryTags)
            .ThenInclude(it => it.Tag)
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .ToListAsync();

        var popularInventories = await query
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
            .ThenInclude(it => it.Tag)
            .OrderByDescending(i => i.Items.Count())
            .Take(5)
            .ToListAsync();

        var tagCloud = (await _context.Tags
            .Select(t => new { t.Name, Count = t.InventoryTags.Count })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ToListAsync())
            .Select(x => new TagCloudItem(x.Name, x.Count))
            .ToList();

        ViewBag.LatestInventories = latestInventories;
        ViewBag.PopularInventories = popularInventories;
        ViewBag.TagCloud = tagCloud;
        ViewBag.SelectedTag = tag?.Trim();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
