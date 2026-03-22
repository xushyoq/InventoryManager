using System.Security.Claims;
using System.Text.RegularExpressions;
using InventoryManager.Data.Repositories;
using InventoryManager.Models;
using InventoryManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManager.Controllers;

public class SupportTicketController : Controller
{
    private readonly ISupportTicketService _supportTicketService;
    private readonly IUserRepository _userRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;

    public SupportTicketController(
        ISupportTicketService supportTicketService,
        IUserRepository userRepository,
        IInventoryRepository inventoryRepository,
        IItemRepository itemRepository)
    {
        _supportTicketService = supportTicketService;
        _userRepository = userRepository;
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string summary, string priority, string? link, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(summary) || summary.Length > 2000)
        {
            TempData["SupportTicketError"] = "Summary is required (max 2000 characters).";
            return RedirectToReferer(link);
        }

        var validPriorities = new[] { "High", "Average", "Low" };
        if (string.IsNullOrWhiteSpace(priority) || !validPriorities.Contains(priority))
        {
            TempData["SupportTicketError"] = "Please select a valid priority.";
            return RedirectToReferer(link);
        }

        string reportedBy;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userRepository.GetByIdAsync(userId, ct);
            reportedBy = user != null ? $"{user.Name} ({user.Email})" : User.Identity?.Name ?? "Unknown";
        }
        else
        {
            reportedBy = "Anonymous";
        }

        var admins = (await _userRepository.GetAllAsync(ct))
            .Where(u => u.IsAdmin && !u.IsBlocked && !string.IsNullOrEmpty(u.Email))
            .Select(u => u.Email!)
            .ToList();

        var inventoryTitle = await ResolveInventoryTitleAsync(link, ct);

        var ticket = new SupportTicketDto
        {
            ReportedBy = reportedBy,
            Inventory = string.IsNullOrWhiteSpace(inventoryTitle) ? null : inventoryTitle,
            Link = link ?? Request.Headers["Referer"].FirstOrDefault() ?? "",
            Priority = priority,
            Summary = summary.Trim(),
            AdminsEmails = admins
        };

        var success = await _supportTicketService.SubmitAsync(ticket, ct);

        if (success)
            TempData["SupportTicketSuccess"] = "Support ticket submitted successfully.";
        else
            TempData["SupportTicketError"] = "Failed to submit the ticket. Please try again.";

        return RedirectToReferer(link);
    }

    private async Task<string?> ResolveInventoryTitleAsync(string? link, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(link)) return null;

        // Match /Inventory/Details?inventoryId=123 or /Inventory/Details/123
        var detailsMatch = Regex.Match(link, @"inventoryId=(\d+)|/Inventory/Details/(\d+)", RegexOptions.IgnoreCase);
        var invIdStr = detailsMatch.Groups[1].Success ? detailsMatch.Groups[1].Value : detailsMatch.Groups[2].Value;
        if (!string.IsNullOrEmpty(invIdStr) && int.TryParse(invIdStr, out var invId1))
        {
            var inv = await _inventoryRepository.GetByIdAsync(invId1, ct);
            return inv?.Name;
        }

        // Match /Item/Index?inventoryId=123
        var itemMatch = Regex.Match(link, @"inventoryId=(\d+)", RegexOptions.IgnoreCase);
        if (itemMatch.Success && int.TryParse(itemMatch.Groups[1].Value, out var invId2))
        {
            var inv = await _inventoryRepository.GetByIdAsync(invId2, ct);
            return inv?.Name;
        }

        // Match /Item/Details/456 - get item's inventory
        var itemDetailsMatch = Regex.Match(link, @"/Item/Details/(\d+)", RegexOptions.IgnoreCase);
        if (itemDetailsMatch.Success && int.TryParse(itemDetailsMatch.Groups[1].Value, out var itemId))
        {
            var item = await _itemRepository.GetByIdWithInventoryAndLikesAsync(itemId, ct);
            return item?.Inventory?.Name;
        }

        return null;
    }

    private IActionResult RedirectToReferer(string? link)
    {
        if (!string.IsNullOrWhiteSpace(link) && Url.IsLocalUrl(link))
            return Redirect(link);
        return RedirectToAction("Index", "Home");
    }
}
