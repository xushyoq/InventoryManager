using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface IInventoryRepository
{
    Task<Inventory?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Inventory>> GetByIdsAsync(int[] ids, CancellationToken ct = default);
    Task<Inventory?> GetByIdWithTagsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Inventory>> GetByCreatorAsync(int userId, CancellationToken ct = default);
    Task<Inventory> AddAsync(Inventory inventory, CancellationToken ct = default);
    Task UpdateAsync(Inventory inventory, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<Inventory> inventories, CancellationToken ct = default);
    Task SetTagsAsync(int inventoryId, IEnumerable<int> tags, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task UpdateFromFormAsync(Inventory existing, Inventory formModel, CancellationToken ct = default);
}