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
}