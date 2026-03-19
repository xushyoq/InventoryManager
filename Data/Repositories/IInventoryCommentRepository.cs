using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface IInventoryCommentRepository
{
    Task<ICollection<InventoryComment>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default);
    Task<InventoryComment> AddAsync(InventoryComment inventoryComment, CancellationToken ct = default);
}