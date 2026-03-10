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
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<InventoryTag> InventoryTags { get; set; } = null!;
    public DbSet<InventoryComment> InventoryComments { get; set; } = null!;
    public DbSet<InventoryAccess> InventoryAccesses { get; set; } = null!;

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

        modelBuilder.Entity<Inventory>()
            .HasGeneratedTsVectorColumn(
                i => i.SearchVector,
                "english",
                i => new { i.Name, i.Description })
            .HasIndex(i => i.SearchVector)
            .HasMethod("GIN");


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

        modelBuilder.Entity<Item>()
            .HasGeneratedTsVectorColumn(
                i => i.SearchVector,
                "english",
                i => new { i.CustomString1, i.CustomString2, i.CustomString3, i.CustomId })
            .HasIndex(i => i.SearchVector)
            .HasMethod("GIN");


        modelBuilder.Entity<InventoryTag>()
            .HasKey(it => new { it.InventoryId, it.TagId });

        modelBuilder.Entity<InventoryTag>()
            .HasOne(it => it.Inventory)
            .WithMany(it => it.InventoryTags)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryTag>()
            .HasOne(it => it.Tag)
            .WithMany(it => it.InventoryTags)
            .HasForeignKey(it => it.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tag>(entity =>
            entity.HasIndex(t => t.Name).IsUnique());

        modelBuilder.Entity<InventoryComment>()
            .HasOne(c => c.Inventory)
            .WithMany()
            .HasForeignKey(c => c.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryComment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<InventoryAccess>()
            .HasOne(a => a.Inventory)
            .WithMany()
            .HasForeignKey(a => a.InventoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryAccess>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InventoryAccess>()
            .HasIndex(a => new { a.InventoryId, a.UserId })
            .IsUnique();
    }
}