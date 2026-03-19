using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class InventoryCommentRepository : IInventoryCommentRepository
{
    private readonly AppDbContext _context;
    public InventoryCommentRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<InventoryComment> AddAsync(InventoryComment inventoryComment, CancellationToken ct = default)
    {
        _context.InventoryComments.Add(inventoryComment);
        await _context.SaveChangesAsync(ct);
        return inventoryComment;
    }

    public async Task<ICollection<InventoryComment>> GetByInventoryIdAsync(int inventoryId, CancellationToken ct = default)
    {
        return await _context.InventoryComments
            .Where(ic => ic.InventoryId == inventoryId)
            .Include(ic => ic.User)
            .OrderBy(ic => ic.CreatedAt)
            .ToListAsync(ct);
    }
}