namespace InventoryManager.Services;

public interface ISalesforceService
{
    Task<bool> CreateAccountAndContactAsync(
        string name,
        string email,
        string phone,
        string company,
        string jobTitle,
        CancellationToken ct = default);
}
