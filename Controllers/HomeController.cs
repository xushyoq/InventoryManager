using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InventoryManager.Models;
using InventoryManager.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var latestInventories = await _context.Inventories
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .ToListAsync();

        var popularInventories = await _context.Inventories
            .Include(i => i.Items)
            .OrderByDescending(i => i.Items.Count())
            .Take(5)
            .ToListAsync();

        ViewBag.LatestInventories = latestInventories;
        ViewBag.PopularInventories = popularInventories;


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
