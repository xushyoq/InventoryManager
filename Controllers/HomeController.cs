using System.Diagnostics;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHomeService _homeService;

    public HomeController(ILogger<HomeController> logger, IHomeService homeService)
    {
        _logger = logger;
        _homeService = homeService;
    }

    public async Task<IActionResult> Index(string? tag, CancellationToken ct)
    {
        var data = await _homeService.GetPageDataAsync(tag, ct);

        ViewBag.LatestInventories = data.LatestInventories;
        ViewBag.PopularInventories = data.PopularInventories;
        ViewBag.TagCloud = data.TagCloud;
        ViewBag.SelectedTag = data.SelectedTag;

        return View();
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
