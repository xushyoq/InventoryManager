using InventoryManager.Data.Repositories;
using InventoryManager.Models;

namespace InventoryManager.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITagRepository _tagRepository;

    public InventoryService(IInventoryRepository inventoryRepository, ITagRepository tagRepository)
    {
        _inventoryRepository = inventoryRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IEnumerable<Inventory>> GetByCreatorAsync(int userId, CancellationToken ct = default)
    {
        return await _inventoryRepository.GetByCreatorAsync(userId, ct);
    }

    public async Task<Inventory?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _inventoryRepository.GetByIdAsync(id, ct);
    }

    public async Task<Inventory?> GetByIdWithTagsAsync(int id, CancellationToken ct = default)
    {
        return await _inventoryRepository.GetByIdWithTagsAsync(id, ct);
    }

    public async Task<Inventory> CreateAsync(Inventory inventory, IReadOnlyList<string> tagNames, int userId, CancellationToken ct = default)
    {
        inventory.CreatedById = userId;
        inventory.CreatedAt = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        var created = await _inventoryRepository.AddAsync(inventory, ct);

        var tagIds = new List<int>();
        var names = tagNames ?? Array.Empty<string>();
        foreach (var name in names)
        {
            var tag = await _tagRepository.GetOrCreateAsync(name, ct);
            tagIds.Add(tag.Id);
        }

        if (tagIds.Count > 0)
        {
            await _inventoryRepository.SetTagsAsync(created.Id, tagIds, ct);
        }

        return created;
    }

    public async Task EditAsync(Inventory inventory, IReadOnlyList<string> tagNames, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var existing = await _inventoryRepository.GetByIdWithTagsAsync(inventory.Id, ct)
            ?? throw new InvalidOperationException("Inventory not found");

        if (!CanEdit(existing, userId, isAdmin))
            throw new UnauthorizedAccessException("Cannot edit this inventory");

        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.CreatedAt = DateTime.SpecifyKind(inventory.CreatedAt, DateTimeKind.Utc);

        var tagIds = new List<int>();
        var names = tagNames ?? Array.Empty<string>();
        foreach (var name in names)
        {
            var tag = await _tagRepository.GetOrCreateAsync(name, ct);
            tagIds.Add(tag.Id);
        }

        await _inventoryRepository.SetTagsAsync(inventory.Id, tagIds, ct);
        await _inventoryRepository.UpdateFromFormAsync(existing, inventory, ct);
    }

    public async Task DeleteAsync(int[] inventoryIds, int userId, bool isAdmin, CancellationToken ct = default)
    {
        var inventories = (await _inventoryRepository.GetByIdsAsync(inventoryIds ?? Array.Empty<int>(), ct)).ToList();
        if (inventories.Count == 0)
            return;

        var toDelete = inventories.Where(i => CanEdit(i, userId, isAdmin)).ToList();
        if (toDelete.Count != inventories.Count)
            throw new UnauthorizedAccessException("Cannot delete some inventories");

        await _inventoryRepository.RemoveRangeAsync(toDelete, ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _inventoryRepository.ExistsAsync(id, ct);
    }

    public async Task<List<(int Id, string Name)>> GetTagSuggestionsAsync(string query, CancellationToken ct = default)
    {
        return await _tagRepository.GetSuggestionsAsync(query, 15, ct);
    }

    private static bool CanEdit(Inventory inventory, int userId, bool isAdmin)
    {
        return inventory.CreatedById == userId || isAdmin;
    }
}