using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TownTrek.Models;

namespace TownTrek.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Town> Towns { get; set; }
        public DbSet<Business> Businesses { get; set; }
        public DbSet<BusinessHour> BusinessHours { get; set; }
        public DbSet<BusinessImage> BusinessImages { get; set; }
        public DbSet<BusinessContact> BusinessContacts { get; set; }
        public DbSet<BusinessService> BusinessServices { get; set; }
        
        // Subscription Management
        public DbSet<SubscriptionTier> SubscriptionTiers { get; set; }
        public DbSet<SubscriptionTierLimit> SubscriptionTierLimits { get; set; }
        public DbSet<SubscriptionTierFeature> SubscriptionTierFeatures { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PriceChangeHistory> PriceChangeHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Town entity
            builder.Entity<Town>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Province).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Landmarks).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => new { e.Name, e.Province }).IsUnique();
            });

            // Configure Business entity
            builder.Entity<Business>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EmailAddress).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.PhysicalAddress).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationships
                entity.HasOne(e => e.Town)
                      .WithMany(t => t.Businesses)
                      .HasForeignKey(e => e.TownId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Businesses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SubscriptionTier
            builder.Entity<SubscriptionTier>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(10,2)");
                
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.SortOrder);
            });

            // Configure SubscriptionTierLimit
            builder.Entity<SubscriptionTierLimit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LimitType).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.SubscriptionTier)
                      .WithMany(t => t.Limits)
                      .HasForeignKey(e => e.SubscriptionTierId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => new { e.SubscriptionTierId, e.LimitType }).IsUnique();
            });

            // Configure SubscriptionTierFeature
            builder.Entity<SubscriptionTierFeature>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FeatureKey).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FeatureName).HasMaxLength(100);
                
                entity.HasOne(e => e.SubscriptionTier)
                      .WithMany(t => t.Features)
                      .HasForeignKey(e => e.SubscriptionTierId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasIndex(e => new { e.SubscriptionTierId, e.FeatureKey }).IsUnique();
            });

            // Configure Subscription
            builder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MonthlyPrice).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.SubscriptionTier)
                      .WithMany(t => t.Subscriptions)
                      .HasForeignKey(e => e.SubscriptionTierId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PriceChangeHistory
            builder.Entity<PriceChangeHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OldPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.NewPrice).HasColumnType("decimal(10,2)");
                
                entity.HasOne(e => e.SubscriptionTier)
                      .WithMany()
                      .HasForeignKey(e => e.SubscriptionTierId)
                      .OnDelete(DeleteBehavior.Restrict);
                      
                entity.HasOne(e => e.ChangedBy)
                      .WithMany()
                      .HasForeignKey(e => e.ChangedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed default data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed default subscription tiers
            builder.Entity<SubscriptionTier>().HasData(
                new SubscriptionTier
                {
                    Id = 1,
                    Name = "Basic",
                    DisplayName = "Basic Plan",
                    Description = "Perfect for small businesses getting started",
                    MonthlyPrice = 199.00m,
                    IsActive = true,
                    SortOrder = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionTier
                {
                    Id = 2,
                    Name = "Standard",
                    DisplayName = "Standard Plan",
                    Description = "Great for growing businesses with multiple locations",
                    MonthlyPrice = 399.00m,
                    IsActive = true,
                    SortOrder = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new SubscriptionTier
                {
                    Id = 3,
                    Name = "Premium",
                    DisplayName = "Premium Plan",
                    Description = "Full-featured plan for established businesses",
                    MonthlyPrice = 599.00m,
                    IsActive = true,
                    SortOrder = 3,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed tier limits
            builder.Entity<SubscriptionTierLimit>().HasData(
                // Basic tier limits
                new SubscriptionTierLimit { Id = 1, SubscriptionTierId = 1, LimitType = "MaxBusinesses", LimitValue = 1 },
                new SubscriptionTierLimit { Id = 2, SubscriptionTierId = 1, LimitType = "MaxImages", LimitValue = 5 },
                new SubscriptionTierLimit { Id = 3, SubscriptionTierId = 1, LimitType = "MaxPDFs", LimitValue = 0 },
                
                // Standard tier limits
                new SubscriptionTierLimit { Id = 4, SubscriptionTierId = 2, LimitType = "MaxBusinesses", LimitValue = 3 },
                new SubscriptionTierLimit { Id = 5, SubscriptionTierId = 2, LimitType = "MaxImages", LimitValue = 15 },
                new SubscriptionTierLimit { Id = 6, SubscriptionTierId = 2, LimitType = "MaxPDFs", LimitValue = 5 },
                
                // Premium tier limits
                new SubscriptionTierLimit { Id = 7, SubscriptionTierId = 3, LimitType = "MaxBusinesses", LimitValue = 10 },
                new SubscriptionTierLimit { Id = 8, SubscriptionTierId = 3, LimitType = "MaxImages", LimitValue = -1 }, // Unlimited
                new SubscriptionTierLimit { Id = 9, SubscriptionTierId = 3, LimitType = "MaxPDFs", LimitValue = -1 }  // Unlimited
            );

            // Seed tier features
            builder.Entity<SubscriptionTierFeature>().HasData(
                // Basic tier features
                new SubscriptionTierFeature { Id = 1, SubscriptionTierId = 1, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
                
                // Standard tier features
                new SubscriptionTierFeature { Id = 2, SubscriptionTierId = 2, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
                new SubscriptionTierFeature { Id = 3, SubscriptionTierId = 2, FeatureKey = "PrioritySupport", IsEnabled = true, FeatureName = "Priority Support" },
                new SubscriptionTierFeature { Id = 4, SubscriptionTierId = 2, FeatureKey = "BasicAnalytics", IsEnabled = true, FeatureName = "Basic Analytics" },
                new SubscriptionTierFeature { Id = 5, SubscriptionTierId = 2, FeatureKey = "PDFUploads", IsEnabled = true, FeatureName = "PDF Document Uploads" },
                
                // Premium tier features
                new SubscriptionTierFeature { Id = 6, SubscriptionTierId = 3, FeatureKey = "BasicSupport", IsEnabled = true, FeatureName = "Standard Support" },
                new SubscriptionTierFeature { Id = 7, SubscriptionTierId = 3, FeatureKey = "PrioritySupport", IsEnabled = true, FeatureName = "Priority Support" },
                new SubscriptionTierFeature { Id = 8, SubscriptionTierId = 3, FeatureKey = "DedicatedSupport", IsEnabled = true, FeatureName = "Dedicated Support" },
                new SubscriptionTierFeature { Id = 9, SubscriptionTierId = 3, FeatureKey = "AdvancedAnalytics", IsEnabled = true, FeatureName = "Advanced Analytics" },
                new SubscriptionTierFeature { Id = 10, SubscriptionTierId = 3, FeatureKey = "FeaturedPlacement", IsEnabled = true, FeatureName = "Featured Placement" },
                new SubscriptionTierFeature { Id = 11, SubscriptionTierId = 3, FeatureKey = "PDFUploads", IsEnabled = true, FeatureName = "PDF Document Uploads" }
            );
        }
    }
}