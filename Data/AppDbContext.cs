using InventoryManager.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Inventory> Inventories { get; set; } = null!;
    public DbSet<Item> Items { get; set; } = null!;

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

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.CreatedBy)
            .WithMany(u => u.OwnedInventories)
            .HasForeignKey(i => i.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Item>()
            .HasOne(it => it.Inventory)
            .WithMany(inv => inv.Items)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Item>()
            .HasOne(it => it.CreatedBy)
            .WithMany(u => u.CreatedItems)
            .HasForeignKey(it => it.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

    }
}