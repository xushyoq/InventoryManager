using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return _context.Users.AnyAsync(u => u.Id == id, ct);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Users
            .OrderBy(u => u.Name)
            .ToListAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var emailTrimmed = email?.Trim();
        if (string.IsNullOrWhiteSpace(emailTrimmed))
        {
            return null;
        }

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == emailTrimmed.ToLowerInvariant(), ct);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Users
            .FindAsync([id], ct);
    }

    public async Task<IEnumerable<User>> GetByIdsAsync(int[] ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Length == 0)
        {
            return new List<User>();
        }

        return await _context.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync(ct);
    }

    public async Task<User?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(providerUserId))
        {
            return null;
        }

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderUserId == providerUserId, ct);
    }

    public async Task RemoveRangeAsync(IEnumerable<User> users, CancellationToken ct = default)
    {
        _context.Users.RemoveRange(users);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<User>> SearchAsync(string query, int limit = 20, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return new List<User>();
        }

        var q = query.Trim().ToLowerInvariant();
        return await _context.Users
            .Where(u => u.Name.ToLower().Contains(q) || (u.Email != null && u.Email.ToLower().Contains(q)))
            .OrderBy(u => u.Name)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }
}