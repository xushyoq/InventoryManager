using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory> AddAsync(Inventory inventory, CancellationToken ct = default)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync(ct);
        return inventory;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Inventories.AnyAsync(i => i.Id == id, ct);
    }

    public async Task<IEnumerable<Inventory>> GetByCreatorAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .Where(i => i.CreatedById == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Inventory?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Inventories
            .Include(i => i.Items)
            .Include(i => i.CreatedBy)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<IEnumerable<Inventory>> GetByIdsAsync(int[] ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<Inventory>();
        }

        return await _context.Inventories
            .Where(i => ids.Contains(i.Id))
            .ToListAsync(ct);
    }

    public async Task<Inventory?> GetByIdWithTagsAsync(int id, CancellationToken ct = default)
    {
        return await _context.Inventories
            .Include(i => i.Items)
            .Include(i => i.CreatedBy)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    }

    public async Task RemoveRangeAsync(IEnumerable<Inventory> inventories, CancellationToken ct)
    {
        _context.Inventories.RemoveRange(inventories);
        await _context.SaveChangesAsync(ct);
    }

    public async Task SetTagsAsync(int inventoryId, IEnumerable<int> tags, CancellationToken ct = default)
    {
        var existing = await _context.InventoryTags
            .Where(it => it.InventoryId == inventoryId)
            .ToListAsync(ct);

        _context.InventoryTags.RemoveRange(existing);

        foreach (var tagId in tags.Distinct())
        {
            _context.InventoryTags.Add(new InventoryTag
            {
                InventoryId = inventoryId,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Inventory inventory, CancellationToken ct = default)
    {
        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync(ct);
    }
}