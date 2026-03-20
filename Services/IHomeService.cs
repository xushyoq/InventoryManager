using InventoryManager.Models;

namespace InventoryManager.Services;

public record HomePageData(
    IEnumerable<Inventory> LatestInventories,
    IEnumerable<Inventory> PopularInventories,
    IEnumerable<TagCloudItem> TagCloud,
    string? SelectedTag);

public interface IHomeService
{
    Task<HomePageData> GetPageDataAsync(string? tag, CancellationToken ct = default);
}
