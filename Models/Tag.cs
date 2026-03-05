using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
}