using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<User>> GetByIdsAsync(int[] ids, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<User>> SearchAsync(string query, int limit = 20, CancellationToken ct = default);
    Task<User> AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task RemoveRangeAsync(IEnumerable<User> users, CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}