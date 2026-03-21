using System.Text.Json.Serialization;

namespace InventoryManager.Models;

public class SupportTicketDto
{
    [JsonPropertyName("ReportedBy")]
    public string ReportedBy { get; set; } = string.Empty;

    [JsonPropertyName("Inventory")]
    public string? Inventory { get; set; }

    [JsonPropertyName("Link")]
    public string Link { get; set; } = string.Empty;

    [JsonPropertyName("Priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonPropertyName("Summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("AdminsEmails")]
    public List<string> AdminsEmails { get; set; } = new();
}
