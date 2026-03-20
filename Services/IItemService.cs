using InventoryManager.Models;

namespace InventoryManager.Services;

public interface IItemService
{
    Task<Item?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Item?> GetByIdWithInventoryAndLikesAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Item>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default);
    Task<Item> CreateAsync(Item item, int inventoryId, int userId, CancellationToken ct = default);
    Task EditAsync(Item item, int userId, bool isAdmin, CancellationToken ct = default);
    Task<int?> DeleteAsync(int[] itemIds, int userId, bool isAdmin, CancellationToken ct = default);
    Task<(bool Liked, int Count)> ToggleLikeAsync(int itemId, int userId, CancellationToken ct = default);
    Task<bool> CanEditItemsAsync(int inventoryId, int userId, bool isAdmin, CancellationToken ct = default);
    Task<bool> CanViewInventoryAsync(int inventoryId, int? userId, bool isAdmin, CancellationToken ct = default);
    Task<Models.Inventory?> GetInventoryAsync(int inventoryId, CancellationToken ct = default);
}