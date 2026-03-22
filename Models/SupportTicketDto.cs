namespace InventoryManager.Models;

public class SupportTicketDto
{
    public string ReportedBy { get; set; } = string.Empty;
    public string? Inventory { get; set; }
    public string Link { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> AdminsEmails { get; set; } = new();
}
