using System.Security.Claims;
using InventoryManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

public class SearchController : Controller
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? q, CancellationToken ct)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = currentUserIdString != null ? int.Parse(currentUserIdString) : null;

        var result = await _searchService.SearchAsync(q, userId, ct);
        return View(result);
    }
}
