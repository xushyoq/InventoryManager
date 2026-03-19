using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface ILikeRepository
{
    Task<Like?> GetByItemAndUserAsync(int itemId, int userId, CancellationToken ct = default);
    Task<Like> AddAsync(Like like, CancellationToken ct = default);
    Task RemoveAsync(Like like, CancellationToken ct = default);
    Task<IEnumerable<int>> GetLikedItemIdsAsync(int userId, IEnumerable<int> itemIds, CancellationToken ct = default);
    Task<int> GetCountByItemIdAsync(int itemId, CancellationToken ct = default);
}