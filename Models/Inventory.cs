using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Features;
using NpgsqlTypes;

namespace InventoryManager.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = "Other";

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsPublic { get; set; } = false;

        public int CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ConcurrencyCheck]
        public int Version { get; set; } = 0;

        public NpgsqlTsVector? SearchVector { get; set; }

        // String fields
        public bool CustomString1State { get; set; } = false;
        public string? CustomString1Name { get; set; }
        public string? CustomString1Description { get; set; }
        public bool CustomString1ShowInTable { get; set; } = true;

        public bool CustomString2State { get; set; } = false;
        public string? CustomString2Name { get; set; }
        public string? CustomString2Description { get; set; }
        public bool CustomString2ShowInTable { get; set; } = true;

        public bool CustomString3State { get; set; } = false;
        public string? CustomString3Name { get; set; }
        public string? CustomString3Description { get; set; }
        public bool CustomString3ShowInTable { get; set; } = true;

        // Multiline text fields
        public bool CustomText1State { get; set; } = false;
        public string? CustomText1Name { get; set; }
        public string? CustomText1Description { get; set; }
        public bool CustomText1ShowInTable { get; set; } = true;

        public bool CustomText2State { get; set; } = false;
        public string? CustomText2Name { get; set; }
        public string? CustomText2Description { get; set; }
        public bool CustomText2ShowInTable { get; set; } = true;

        public bool CustomText3State { get; set; } = false;
        public string? CustomText3Name { get; set; }
        public string? CustomText3Description { get; set; }
        public bool CustomText3ShowInTable { get; set; } = true;

        // Number fields
        public bool CustomInt1State { get; set; } = false;
        public string? CustomInt1Name { get; set; }
        public string? CustomInt1Description { get; set; }
        public bool CustomInt1ShowInTable { get; set; } = true;

        public bool CustomInt2State { get; set; } = false;
        public string? CustomInt2Name { get; set; }
        public string? CustomInt2Description { get; set; }
        public bool CustomInt2ShowInTable { get; set; } = true;

        public bool CustomInt3State { get; set; } = false;
        public string? CustomInt3Name { get; set; }
        public string? CustomInt3Description { get; set; }
        public bool CustomInt3ShowInTable { get; set; } = true;

        // Checkbox fields
        public bool CustomBool1State { get; set; } = false;
        public string? CustomBool1Name { get; set; }
        public string? CustomBool1Description { get; set; }
        public bool CustomBool1ShowInTable { get; set; } = true;

        public bool CustomBool2State { get; set; } = false;
        public string? CustomBool2Name { get; set; }
        public string? CustomBool2Description { get; set; }
        public bool CustomBool2ShowInTable { get; set; } = true;

        public bool CustomBool3State { get; set; } = false;
        public string? CustomBool3Name { get; set; }
        public string? CustomBool3Description { get; set; }
        public bool CustomBool3ShowInTable { get; set; } = true;

        // Link fields
        public bool CustomLink1State { get; set; } = false;
        public string? CustomLink1Name { get; set; }
        public string? CustomLink1Description { get; set; }
        public bool CustomLink1ShowInTable { get; set; } = true;

        public bool CustomLink2State { get; set; } = false;
        public string? CustomLink2Name { get; set; }
        public string? CustomLink2Description { get; set; }
        public bool CustomLink2ShowInTable { get; set; } = true;

        public bool CustomLink3State { get; set; } = false;
        public string? CustomLink3Name { get; set; }
        public string? CustomLink3Description { get; set; }
        public bool CustomLink3ShowInTable { get; set; } = true;

        // Navigation
        public ICollection<Item> Items { get; set; } = new List<Item>();
        public ICollection<InventoryTag> InventoryTags { get; set; } = new List<InventoryTag>();
    }
}