using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;

    public TagRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var nameLower = name.Trim().ToLowerInvariant();

        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.ToLower() == nameLower, ct);
    }

    public async Task<Tag> GetOrCreateAsync(string name, CancellationToken ct = default)
    {
        var nameNormalized = name.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(nameNormalized))
        {
            throw new ArgumentException("Tag cannot be empty.", nameof(name));
        }

        var tag = await GetByNameAsync(nameNormalized, ct);

        if (tag != null)
        {
            return tag;
        }

        tag = new Tag { Name = nameNormalized };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task<List<(int Id, string Name)>> GetSuggestionsAsync(string query, int limit = 15, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<(int, string)>();
        }

        var queryLower = query.Trim().ToLowerInvariant();
        var items = await _context.Tags
            .Where(t => t.Name.ToLower().Contains(queryLower))
            .OrderBy(t => t.Name)
            .Take(limit)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        return items.Select(x => (x.Id, x.Name)).ToList();
    }

    public async Task<List<(string Name, int Count)>> GetTagCloudAsync(CancellationToken ct = default)
    {
        var items = await _context.Tags
            .Select(t => new { t.Name, Count = t.InventoryTags.Count })
            .Where(x => x.Count > 0)
            .OrderByDescending(x => x.Count)
            .ToListAsync(ct);

        return items.Select(x => (x.Name, x.Count)).ToList();
    }
}