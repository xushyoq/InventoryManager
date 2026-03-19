using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface IInventoryAccessRepository
{
    Task<ICollection<InventoryAccess>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default);
    Task<InventoryAccess?> GetByInventoryAndUserAsync(int inventoryId, int userId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int inventoryId, int userId, CancellationToken ct = default);
    Task<InventoryAccess> AddAsync(InventoryAccess inventoryAccess, CancellationToken ct = default);
    Task RemoveAsync(InventoryAccess inventoryAccess, CancellationToken ct = default);
    Task<bool> RevokeAccessAsync(int inventoryId, int userId, CancellationToken ct = default);
}