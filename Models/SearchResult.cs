namespace InventoryManager.Models;

public class SearchResult
{
    public string? Query { get; set; }
    public List<Inventory> Inventories { get; set; } = [];
    public List<Item> Items { get; set; } = [];
}