using InventoryManager.Data.Repositories;
using InventoryManager.Models;

namespace InventoryManager.Services;

public class HomeService : IHomeService
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ITagRepository _tagRepository;

    public HomeService(IInventoryRepository inventoryRepository, ITagRepository tagRepository)
    {
        _inventoryRepository = inventoryRepository;
        _tagRepository = tagRepository;
    }

    public async Task<HomePageData> GetPageDataAsync(string? tag, CancellationToken ct = default)
    {
        var tagTrimmed = string.IsNullOrWhiteSpace(tag) ? null : tag.Trim();

        var latest = await _inventoryRepository.GetLatestAsync(tagTrimmed, 10, ct);
        var popular = await _inventoryRepository.GetMostLikedAsync(tagTrimmed, 5, ct);
        var tagCloudRaw = await _tagRepository.GetTagCloudAsync(ct);
        var tagCloud = tagCloudRaw.Select(x => new TagCloudItem(x.Name, x.Count)).ToList();

        return new HomePageData(latest, popular, tagCloud, tagTrimmed);
    }
}
