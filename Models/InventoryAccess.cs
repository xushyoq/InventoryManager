namespace InventoryManager.Models;

public class InventoryAccess
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}
