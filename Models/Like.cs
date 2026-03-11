namespace InventoryManager.Models;

public class Like
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
