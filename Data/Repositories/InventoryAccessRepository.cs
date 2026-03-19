using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class InventoryAccessRepository : IInventoryAccessRepository
{
    private readonly AppDbContext _context;
    public InventoryAccessRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<InventoryAccess> AddAsync(InventoryAccess inventoryAccess, CancellationToken ct = default)
    {
        _context.InventoryAccesses.Add(inventoryAccess);
        await _context.SaveChangesAsync(ct);
        return inventoryAccess;
    }

    public async Task<bool> ExistsAsync(int inventoryId, int userId, CancellationToken ct = default)
    {
        return await _context.InventoryAccesses.AnyAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId, ct);
    }

    public async Task<InventoryAccess?> GetByInventoryAndUserAsync(int inventoryId, int userId, CancellationToken ct = default)
    {
        return await _context.InventoryAccesses.FirstOrDefaultAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId, ct);
    }

    public async Task<ICollection<InventoryAccess>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default)
    {
        return await _context.InventoryAccesses
            .Include(ia => ia.User)
            .Where(ia => ia.InventoryId == inventoryId)
            .OrderBy(ia => ia.GrantedAt)
            .ToListAsync(ct);
    }

    public async Task RemoveAsync(InventoryAccess inventoryAccess, CancellationToken ct = default)
    {
        _context.InventoryAccesses.Remove(inventoryAccess);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> RevokeAccessAsync(int inventoryId, int userId, CancellationToken ct = default)
    {
        var access = await _context.InventoryAccesses
            .FirstOrDefaultAsync(ia => ia.InventoryId == inventoryId && ia.UserId == userId, ct);
        if (access == null)
        {
            return false;
        }

        _context.InventoryAccesses.Remove(access);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}