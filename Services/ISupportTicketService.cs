using InventoryManager.Models;

namespace InventoryManager.Services;

public interface ISupportTicketService
{
    Task<bool> SubmitAsync(SupportTicketDto ticket, CancellationToken ct = default);
}
