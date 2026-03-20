using InventoryManager.Models;

namespace InventoryManager.Services;

public interface ISearchService
{
    Task<SearchResult> SearchAsync(string? query, int? userId, CancellationToken ct = default);
}
