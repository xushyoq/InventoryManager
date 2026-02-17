using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Name).HasMaxLength(500);
            entity.Property(u => u.Email).HasMaxLength(255);
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            entity.Property(u => u.Provider).HasMaxLength(255);
            entity.Property(u => u.ProviderUserId).HasMaxLength(500);
        });
    }
}