using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models
{
    public class Inventory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; } // Markdown

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

        public int Version { get; set; } = 0; // Optimistic locking

        // String fields
        public bool CustomString1State { get; set; } = false;
        public string? CustomString1Name { get; set; }

        public bool CustomString2State { get; set; } = false;
        public string? CustomString2Name { get; set; }

        public bool CustomString3State { get; set; } = false;
        public string? CustomString3Name { get; set; }

        // Multiline text fields
        public bool CustomText1State { get; set; } = false;
        public string? CustomText1Name { get; set; }

        public bool CustomText2State { get; set; } = false;
        public string? CustomText2Name { get; set; }

        public bool CustomText3State { get; set; } = false;
        public string? CustomText3Name { get; set; }

        // Number fields
        public bool CustomInt1State { get; set; } = false;
        public string? CustomInt1Name { get; set; }

        public bool CustomInt2State { get; set; } = false;
        public string? CustomInt2Name { get; set; }

        public bool CustomInt3State { get; set; } = false;
        public string? CustomInt3Name { get; set; }

        // Checkbox fields
        public bool CustomBool1State { get; set; } = false;
        public string? CustomBool1Name { get; set; }

        public bool CustomBool2State { get; set; } = false;
        public string? CustomBool2Name { get; set; }

        public bool CustomBool3State { get; set; } = false;
        public string? CustomBool3Name { get; set; }

        // Link fields
        public bool CustomLink1State { get; set; } = false;
        public string? CustomLink1Name { get; set; }

        public bool CustomLink2State { get; set; } = false;
        public string? CustomLink2Name { get; set; }

        public bool CustomLink3State { get; set; } = false;
        public string? CustomLink3Name { get; set; }

        // Navigation

    }
}