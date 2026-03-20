using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Item> AddAsync(Item item, CancellationToken ct = default)
    {
        _context.Items.Add(item);
        await _context.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Items.AnyAsync(i => i.Id == id, ct);
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Items
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<IEnumerable<Item>> GetByIdsAsync(int[] ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<Item>();
        }

        return await _context.Items
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
    }

    public async Task<Item?> GetByIdWithLikesAsync(int id, CancellationToken ct = default)
    {
        return await _context.Items
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<IEnumerable<Item>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default)
    {
        return await _context.Items
            .Include(i => i.Likes)
            .Where(i => i.InventoryId == inventoryId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task RemoveRangeAsync(IEnumerable<Item> items, CancellationToken ct = default)
    {
        _context.Items.RemoveRange(items);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Item item, CancellationToken ct = default)
    {
        _context.Items.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Item?> GetByIdWithInventoryAndLikesAsync(int id, CancellationToken ct = default)
    {
        return await _context.Items
            .Include(i => i.Inventory)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task UpdateFromFormAsync(Item existing, Item formModel, CancellationToken ct = default)
    {
        _context.Entry(existing).CurrentValues.SetValues(formModel);
        existing.Version++;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Item>> SearchAsync(string query, IReadOnlyList<int> visibleInventoryIds, CancellationToken ct = default)
    {
        if (visibleInventoryIds == null || visibleInventoryIds.Count == 0)
            return Array.Empty<Item>();

        var queryTrimmed = query.Trim();
        var queryLower = queryTrimmed.ToLowerInvariant();

        var byFts = await _context.Items
            .Include(i => i.Inventory)
            .Where(i => visibleInventoryIds.Contains(i.InventoryId) &&
                        i.SearchVector != null &&
                        i.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", queryTrimmed)))
            .Take(50)
            .ToListAsync(ct);

        var tagMatchInventoryIds = await _context.InventoryTags
            .Where(it => it.Tag != null && it.Tag.Name.ToLower().Contains(queryLower)
                         && visibleInventoryIds.Contains(it.InventoryId))
            .Select(it => it.InventoryId)
            .Distinct()
            .ToListAsync(ct);

        var byTag = tagMatchInventoryIds.Count > 0
            ? await _context.Items
                .Include(i => i.Inventory)
                .Where(i => tagMatchInventoryIds.Contains(i.InventoryId))
                .Take(50)
                .ToListAsync(ct)
            : new List<Item>();

        return byFts.UnionBy(byTag, i => i.Id).Take(50).ToList();
    }

    public async Task<bool> IsCustomIdTakenAsync(int inventoryId, string customId, int? excludeItemId = null, CancellationToken ct = default)
    {
        return await _context.Items
            .AnyAsync(i => i.InventoryId == inventoryId
                        && i.CustomId == customId
                        && (excludeItemId == null || i.Id != excludeItemId), ct);
    }
}