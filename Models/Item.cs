using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models;

public class Item
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public Inventory Inventory { get; set; }


    [Required]
    [MaxLength(255)]
    public string CustomId { get; set; } = string.Empty;


    [MaxLength(500)]
    public string? CustomString1 { get; set; }
    [MaxLength(500)]
    public string? CustomString2 { get; set; }
    [MaxLength(500)]
    public string? CustomString3 { get; set; }


    public string? CustomText1 { get; set; }
    public string? CustomText2 { get; set; }
    public string? CustomText3 { get; set; }

    public int CustomInt1 { get; set; }
    public int CustomInt2 { get; set; }
    public int CustomInt3 { get; set; }

    public bool CustomBool1 { get; set; }
    public bool CustomBool2 { get; set; }
    public bool CustomBool3 { get; set; }


    [MaxLength(500)]
    public string? CustomLink1 { get; set; }
    [MaxLength(500)]
    public string? CustomLink2 { get; set; }
    [MaxLength(500)]
    public string? CustomLink3 { get; set; }


    public int CreatedById { get; set; }
    public User CreatedBy { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int Version { get; set; } = 0;

    //public ICollection<Like> Likes { get; set; } = new List<Like>();
}