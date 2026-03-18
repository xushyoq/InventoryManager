using InventoryManager.Models;

namespace InventoryManager.Data.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken ct = default);

    Task<Tag> GetOrCreateAsync(string name, CancellationToken ct = default);

    Task<List<(int Id, string Name)>> GetSuggestionsAsync(string query, int limit = 15, CancellationToken ct = default);

    Task<List<(string Name, int Count)>> GetTagCloudAsync(CancellationToken ct = default);
}