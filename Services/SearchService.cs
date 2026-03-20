using InventoryManager.Data.Repositories;
using InventoryManager.Models;

namespace InventoryManager.Services;

public class SearchService : ISearchService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IItemRepository _itemRepository;

    public SearchService(IInventoryRepository inventoryRepository, IItemRepository itemRepository)
    {
        _inventoryRepository = inventoryRepository;
        _itemRepository = itemRepository;
    }

    public async Task<SearchResult> SearchAsync(string? query, int? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new SearchResult();

        var inventories = (await _inventoryRepository.SearchAsync(query, userId, ct)).ToList();

        var visibleInventoryIds = inventories.Select(i => i.Id).ToList();
        var items = (await _itemRepository.SearchAsync(query, visibleInventoryIds, ct)).ToList();

        return new SearchResult
        {
            Query = query,
            Inventories = inventories,
            Items = items
        };
    }
}
