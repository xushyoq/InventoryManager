using InventoryManager.Models;

namespace InventoryManager.Services;

public interface IInventoryService
{
    Task<IEnumerable<Inventory>> GetByCreatorAsync(int userId, CancellationToken ct = default);
    Task<Inventory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Inventory?> GetByIdWithTagsAsync(int id, CancellationToken ct = default);
    Task<Inventory> CreateAsync(Inventory inventory, IReadOnlyList<string> tagNames, int userId, CancellationToken ct = default);
    Task EditAsync(Inventory inventory, IReadOnlyList<string> tagNames, int userId, bool isAdmin, CancellationToken ct = default);
    Task DeleteAsync(int[] inventoryIds, int userId, bool isAdmin, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<List<(int Id, string Name)>> GetTagSuggestionsAsync(string query, CancellationToken ct = default);
}