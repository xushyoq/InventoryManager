using InventoryManager.Models;
using InventoryManager.Services;
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

    public async Task UpdateFromFormAsync(Inventory existing, Inventory formModel, CancellationToken ct = default)
    {
        _context.Entry(existing).CurrentValues.SetValues(formModel);
        existing.UpdatedAt = DateTime.UtcNow;
        existing.Version++;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetLatestAsync(string? tag, int limit, CancellationToken ct = default)
    {
        var query = _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagLower = tag.Trim().ToLowerInvariant();
            query = query.Where(i => i.InventoryTags.Any(it => it.Tag != null && it.Tag.Name.ToLower() == tagLower));
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetMostLikedAsync(string? tag, int limit, CancellationToken ct = default)
    {
        var query = _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagLower = tag.Trim().ToLowerInvariant();
            query = query.Where(i => i.InventoryTags.Any(it => it.Tag != null && it.Tag.Name.ToLower() == tagLower));
        }

        return await query
            .OrderByDescending(i => i.Items.Count)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetLatestPublicAsync(string? tag, int limit, CancellationToken ct = default)
    {
        var query = _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .Where(i => i.IsPublic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagLower = tag.Trim().ToLowerInvariant();
            query = query.Where(i => i.InventoryTags.Any(it => it.Tag != null && it.Tag.Name.ToLower() == tagLower));
        }

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetMostPopularAsync(string? tag, int limit, CancellationToken ct = default)
    {
        var query = _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .Where(i => i.IsPublic)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var tagLower = tag.Trim().ToLowerInvariant();
            query = query.Where(i => i.InventoryTags.Any(it => it.Tag != null && it.Tag.Name.ToLower() == tagLower));
        }

        return await query
            .OrderByDescending(i => i.Items.Count)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetByUserAccessAsync(int userId, CancellationToken ct = default)
    {
        var inventoryIds = await _context.InventoryAccesses
            .Where(a => a.UserId == userId)
            .Select(a => a.InventoryId)
            .ToListAsync(ct);

        return await _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .Where(i => inventoryIds.Contains(i.Id))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> GetAllWithTagsAsync(CancellationToken ct = default)
    {
        return await _context.Inventories
            .Include(i => i.CreatedBy)
            .Include(i => i.Items)
            .Include(i => i.InventoryTags)
                .ThenInclude(it => it.Tag)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Inventory>> SearchAsync(string query, int? userId, CancellationToken ct = default)
    {
        var visibleQuery = _context.Inventories.AsQueryable();
        if (userId.HasValue)
            visibleQuery = visibleQuery.Where(i => i.IsPublic || i.CreatedById == userId.Value);
        else
            visibleQuery = visibleQuery.Where(i => i.IsPublic);

        var queryTrimmed = query.Trim();
        var queryLower = queryTrimmed.ToLowerInvariant();

        var byFts = await visibleQuery
            .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
            .Where(i => i.SearchVector != null && i.SearchVector.Matches(EF.Functions.PlainToTsQuery("english", queryTrimmed)))
            .Take(20)
            .ToListAsync(ct);

        var tagMatchIds = await _context.InventoryTags
            .Where(it => it.Tag != null && it.Tag.Name.ToLower().Contains(queryLower))
            .Select(it => it.InventoryId)
            .Distinct()
            .ToListAsync(ct);

        var visibleIds = await visibleQuery.Select(i => i.Id).ToListAsync(ct);
        var visibleTagIds = tagMatchIds.Where(id => visibleIds.Contains(id)).ToList();

        var byTag = visibleTagIds.Count > 0
            ? await visibleQuery
                .Include(i => i.InventoryTags).ThenInclude(it => it.Tag)
                .Where(i => visibleTagIds.Contains(i.Id))
                .ToListAsync(ct)
            : new List<Inventory>();

        return byFts.UnionBy(byTag, i => i.Id).Take(20).ToList();
    }

    public async Task<string> AllocateNextCustomIdAsync(int inventoryId, CancellationToken ct = default)
    {
        const int maxRetries = 10;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var inventory = await _context.Inventories
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == inventoryId, ct)
                ?? throw new InvalidOperationException("Inventory not found");

            if (string.IsNullOrWhiteSpace(inventory.CustomIdTemplate))
                throw new InvalidOperationException("Inventory has no CustomIdTemplate configured");

            var customId = CustomIdGenerator.Generate(inventory.CustomIdTemplate, inventory.CustomIdCounter);
            var nextCounter = inventory.CustomIdCounter + 1;

            // Atomic increment: WHERE Id = X AND CustomIdCounter = expectedValue
            var updated = await _context.Inventories
                .Where(i => i.Id == inventoryId && i.CustomIdCounter == inventory.CustomIdCounter)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.CustomIdCounter, nextCounter), ct);

            if (updated == 1)
                return customId;

            // Another request incremented the counter concurrently — retry
        }

        throw new InvalidOperationException($"Failed to allocate custom ID after {maxRetries} retries");
    }
}