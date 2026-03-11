using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models;

public class InventoryComment
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
