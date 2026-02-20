using System.ComponentModel.DataAnnotations;

namespace InventoryManager.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public bool IsAdmin { get; set; } = false;
    public bool IsBlcoked { get; set; } = false;

    [MaxLength(255)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(500)]
    public string ProviderUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}