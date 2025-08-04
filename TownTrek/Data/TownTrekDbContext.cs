using Microsoft.EntityFrameworkCore;
using TownTrek.Models;

namespace TownTrek.Data;

public class TownTrekDbContext : DbContext
{
    public TownTrekDbContext(DbContextOptions<TownTrekDbContext> options) : base(options)
    {
    }

    public DbSet<Town> Towns { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Town entity
        modelBuilder.Entity<Town>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Add index for better performance on common queries
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.State, e.Country });
        });
    }
}