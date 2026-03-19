using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly AppDbContext _context;
    public LikeRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Like> AddAsync(Like like, CancellationToken ct = default)
    {
        _context.Likes.Add(like);
        await _context.SaveChangesAsync(ct);
        return like;
    }

    public async Task<Like?> GetByItemAndUserAsync(int itemId, int userId, CancellationToken ct = default)
    {
        return await _context.Likes.FirstOrDefaultAsync(l => l.ItemId == itemId && l.UserId == userId, ct);
    }

    public async Task<int> GetCountByItemIdAsync(int itemId, CancellationToken ct = default)
    {
        return await _context.Likes
            .CountAsync(l => l.ItemId == itemId, ct);
    }

    public async Task<IEnumerable<int>> GetLikedItemIdsAsync(int userId, IEnumerable<int> itemIds, CancellationToken ct = default)
    {
        var ids = itemIds?.Where(id => id > 0).Distinct().ToList();
        if (ids == null || ids.Count == 0)
        {
            return Array.Empty<int>();
        }

        return await _context.Likes
            .Where(l => l.UserId == userId && ids.Contains(l.ItemId))
            .Select(l => l.ItemId)
            .ToListAsync(ct);
    }

    public async Task RemoveAsync(Like like, CancellationToken ct = default)
    {
        _context.Likes.Remove(like);
        await _context.SaveChangesAsync(ct);
    }
}