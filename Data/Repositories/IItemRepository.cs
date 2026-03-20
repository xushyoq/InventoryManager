using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface IItemRepository
{
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Item>> GetByIdsAsync(int[] ids, CancellationToken ct = default);
    Task<Item?> GetByIdWithLikesAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Item>> GetByInventoryIdAsync(int id, CancellationToken ct = default);
    Task<Item> AddAsync(Item item, CancellationToken ct = default);
    Task UpdateAsync(Item item, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Item> items, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<Item?> GetByIdWithInventoryAndLikesAsync(int id, CancellationToken ct = default);
    Task UpdateFromFormAsync(Item existing, Item formModel, CancellationToken ct = default);
    Task<IEnumerable<Item>> SearchAsync(string query, IReadOnlyList<int> visibleInventoryIds, CancellationToken ct = default);
    Task<bool> IsCustomIdTakenAsync(int inventoryId, string customId, int? excludeItemId = null, CancellationToken ct = default);
}