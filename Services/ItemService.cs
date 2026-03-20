using InventoryManager.Data.Repositories;
using InventoryManager.Models;

namespace InventoryManager.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryAccessRepository _accessRepository;
    private readonly ILikeRepository _likeRepository;

    public ItemService(
        IItemRepository itemRepository,
        IInventoryRepository inventoryRepository,
        IInventoryAccessRepository accessRepository,
        ILikeRepository likeRepository)
    {
        _itemRepository = itemRepository;
        _inventoryRepository = inventoryRepository;
        _accessRepository = accessRepository;
        _likeRepository = likeRepository;
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _itemRepository.GetByIdAsync(id, ct);
    }

    public async Task<Item?> GetByIdWithInventoryAndLikesAsync(int id, CancellationToken ct = default)
    {
        return await _itemRepository.GetByIdWithInventoryAndLikesAsync(id, ct);
    }

    public async Task<IEnumerable<Item>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default)
    {
        return await _itemRepository.GetByInventoryIdAsync(inventoryId, ct);
    }

    public async Task<Item> CreateAsync(Item item, int inventoryId, int userId, CancellationToken ct = default)
    {
        item.CreatedById = userId;
        item.CreatedAt = DateTime.UtcNow;
        item.InventoryId = inventoryId;

        // Auto-generate CustomId if the inventory has a template and the user didn't supply one
        if (string.IsNullOrWhiteSpace(item.CustomId))
        {
            var inventory = await _inventoryRepository.GetByIdAsync(inventoryId, ct);
            if (inventory != null && !string.IsNullOrWhiteSpace(inventory.CustomIdTemplate))
                item.CustomId = await _inventoryRepository.AllocateNextCustomIdAsync(inventoryId, ct);
        }
        else
        {
            // User supplied a custom ID — validate uniqueness
            if (await _itemRepository.IsCustomIdTakenAsync(inventoryId, item.CustomId, null, ct))
                throw new InvalidOperationException($"Custom ID '{item.CustomId}' is already used in this inventory");
        }

        return await _itemRepository.AddAsync(item, ct);
    }

    public async Task EditAsync(Item item, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var existing = await _itemRepository.GetByIdAsync(item.Id, ct)
            ?? throw new InvalidOperationException("Item not found");

        if (!await CanEditAsync(existing.InventoryId, userId, isAdmin, ct))
            throw new UnauthorizedAccessException("Cannot edit this item");

        // If the user changed the CustomId, validate uniqueness (exclude the current item)
        if (!string.IsNullOrWhiteSpace(item.CustomId) && item.CustomId != existing.CustomId)
        {
            if (await _itemRepository.IsCustomIdTakenAsync(existing.InventoryId, item.CustomId, existing.Id, ct))
                throw new InvalidOperationException($"Custom ID '{item.CustomId}' is already used in this inventory");
        }

        item.CreatedAt = DateTime.SpecifyKind(item.CreatedAt, DateTimeKind.Utc);
        await _itemRepository.UpdateFromFormAsync(existing, item, ct);
    }

    public async Task<int?> DeleteAsync(int[] itemIds, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var ids = itemIds ?? Array.Empty<int>();
        if (ids.Length == 0)
            return null;

        var items = (await _itemRepository.GetByIdsAsync(ids, ct)).ToList();
        if (items.Count == 0)
            return null;

        var inventoryId = items[0].InventoryId;
        if (!await CanEditAsync(inventoryId, userId, isAdmin, ct))
            throw new UnauthorizedAccessException("Cannot delete these items");

        await _itemRepository.RemoveRangeAsync(items, ct);
        return inventoryId;
    }

    public async Task<(bool Liked, int Count)> ToggleLikeAsync(int itemId, int userId, CancellationToken ct = default)
    {
        var item = await _itemRepository.GetByIdWithInventoryAndLikesAsync(itemId, ct);
        if (item == null || item.Inventory == null)
            throw new InvalidOperationException("Item not found");

        var existingLike = await _likeRepository.GetByItemAndUserAsync(itemId, userId, ct);

        if (existingLike != null)
        {
            await _likeRepository.RemoveAsync(existingLike, ct);
        }
        else
        {
            await _likeRepository.AddAsync(new Like { ItemId = itemId, UserId = userId }, ct);
        }

        var count = await _likeRepository.GetCountByItemIdAsync(itemId, ct);
        var liked = existingLike == null;
        return (liked, count);
    }

    public async Task<bool> CanEditItemsAsync(int inventoryId, int userId, bool isAdmin, CancellationToken ct = default)
    {
        return await CanEditAsync(inventoryId, userId, isAdmin, ct);
    }

    public async Task<bool> CanViewInventoryAsync(int inventoryId, int? userId, bool isAdmin, CancellationToken ct = default)
    {
        return await _inventoryRepository.ExistsAsync(inventoryId, ct);
    }

    public async Task<Models.Inventory?> GetInventoryAsync(int inventoryId, CancellationToken ct = default)
    {
        return await _inventoryRepository.GetByIdAsync(inventoryId, ct);
    }

    private async Task<bool> CanEditAsync(int inventoryId, int userId, bool isAdmin, CancellationToken ct)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(inventoryId, ct);
        if (inventory == null)
            return false;

        // Public inventory: any authenticated user can manage items
        if (inventory.IsPublic)
            return true;

        // Private inventory: owner, admin, or explicitly granted access
        if (inventory.CreatedById == userId || isAdmin)
            return true;

        return await _accessRepository.ExistsAsync(inventoryId, userId, ct);
    }
}