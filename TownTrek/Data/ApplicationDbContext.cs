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
        public DbSet<BusinessCategory> BusinessCategories { get; set; }
        public DbSet<BusinessSubCategory> BusinessSubCategories { get; set; }
        public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }
        
        // Category-specific details
        public DbSet<MarketDetails> MarketDetails { get; set; }
        public DbSet<TourDetails> TourDetails { get; set; }
        public DbSet<EventDetails> EventDetails { get; set; }
        public DbSet<RestaurantDetails> RestaurantDetails { get; set; }
        public DbSet<AccommodationDetails> AccommodationDetails { get; set; }
        public DbSet<ShopDetails> ShopDetails { get; set; }
        
        // Notifications and special hours
        public DbSet<BusinessAlert> BusinessAlerts { get; set; }
        public DbSet<SpecialOperatingHours> SpecialOperatingHours { get; set; }
        
        // Subscription Management
        public DbSet<SubscriptionTier> SubscriptionTiers { get; set; }
        public DbSet<SubscriptionTierLimit> SubscriptionTierLimits { get; set; }
        public DbSet<SubscriptionTierFeature> SubscriptionTierFeatures { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PriceChangeHistory> PriceChangeHistory { get; set; }
        
        // Member Features
        public DbSet<BusinessReview> BusinessReviews { get; set; }
        public DbSet<BusinessReviewResponse> BusinessReviewResponses { get; set; }
        public DbSet<FavoriteBusiness> FavoriteBusinesses { get; set; }
        
        // Analytics
        public DbSet<BusinessViewLog> BusinessViewLogs { get; set; }
        public DbSet<AnalyticsSnapshot> AnalyticsSnapshots { get; set; }
        
        // Trial Security
        public DbSet<TrialAuditLog> TrialAuditLogs { get; set; }
        
        // Error Logging
        public DbSet<ErrorLogEntry> ErrorLogs { get; set; }
        
        // Admin Messages
        public DbSet<AdminMessageTopic> AdminMessageTopics { get; set; }
        public DbSet<AdminMessage> AdminMessages { get; set; }
        
        // Analytics Audit Logs
        public DbSet<AnalyticsAuditLog> AnalyticsAuditLogs { get; set; }
        
        // Analytics Monitoring and Observability
        public DbSet<AnalyticsPerformanceLog> AnalyticsPerformanceLogs { get; set; }
        public DbSet<AnalyticsErrorLog> AnalyticsErrorLogs { get; set; }
        public DbSet<AnalyticsUsageLog> AnalyticsUsageLogs { get; set; }
        
        // Analytics Export and Sharing
        public DbSet<AnalyticsShareableLink> AnalyticsShareableLinks { get; set; }
        public DbSet<AnalyticsEmailReport> AnalyticsEmailReports { get; set; }
        
        // Advanced Analytics
        public DbSet<CustomMetric> CustomMetrics { get; set; }
        public DbSet<CustomMetricDataPoint> CustomMetricDataPoints { get; set; }
        public DbSet<CustomMetricGoal> CustomMetricGoals { get; set; }
        public DbSet<AnomalyDetection> AnomalyDetections { get; set; }
        public DbSet<PredictiveForecast> PredictiveForecasts { get; set; }
        public DbSet<SeasonalPattern> SeasonalPatterns { get; set; }
        
        // Dashboard Customization
        public DbSet<DashboardPreferences> DashboardPreferences { get; set; }
        public DbSet<SavedDashboardView> SavedDashboardViews { get; set; }
        
        // Analytics Events (Event Sourcing)
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }

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

            // Configure category-specific details
            ConfigureCategorySpecificEntities(builder);
            
            // Configure member features
            ConfigureMemberEntities(builder);
            
            // Configure trial security
            ConfigureTrialEntities(builder);
            
            // Configure error logging
            ConfigureErrorLoggingEntities(builder);
            
            // Configure admin messages
            ConfigureAdminMessageEntities(builder);
            ConfigureAnalyticsAuditLogEntity(builder);
            ConfigureAnalyticsMonitoringEntities(builder);
            ConfigureAnalyticsEntities(builder);
            
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

            // Seed business categories and subcategories (phase 1)
            builder.Entity<BusinessCategory>().HasData(
                new BusinessCategory { Id = 1, Key = "shops-retail", Name = "Shops & Retail", Description = "Local shops and retail businesses", IconClass = "fas fa-shopping-bag", IsActive = true, FormType = Models.BusinessFormType.Shop },
                new BusinessCategory { Id = 2, Key = "restaurants-food", Name = "Restaurants & Food Services", Description = "Restaurants, cafes, and food services", IconClass = "fas fa-utensils", IsActive = true, FormType = Models.BusinessFormType.Restaurant },
                new BusinessCategory { Id = 3, Key = "markets-vendors", Name = "Markets & Vendors", Description = "Local markets and vendor stalls", IconClass = "fas fa-store", IsActive = true, FormType = Models.BusinessFormType.Market },
                new BusinessCategory { Id = 4, Key = "accommodation", Name = "Accommodation", Description = "Hotels, guesthouses, and lodging", IconClass = "fas fa-bed", IsActive = true, FormType = Models.BusinessFormType.Accommodation },
                new BusinessCategory { Id = 5, Key = "tours-experiences", Name = "Tours & Experiences", Description = "Tour guides and experience providers", IconClass = "fas fa-map-marked-alt", IsActive = true, FormType = Models.BusinessFormType.Tour },
                new BusinessCategory { Id = 6, Key = "events", Name = "Events", Description = "Local events and entertainment", IconClass = "fas fa-calendar-alt", IsActive = true, FormType = Models.BusinessFormType.Event }
            );

            builder.Entity<BusinessSubCategory>().HasData(
                // shops-retail
                new BusinessSubCategory { Id = 1, CategoryId = 1, Key = "clothing", Name = "Clothing & Fashion", IsActive = true },
                new BusinessSubCategory { Id = 2, CategoryId = 1, Key = "electronics", Name = "Electronics", IsActive = true },
                new BusinessSubCategory { Id = 3, CategoryId = 1, Key = "books", Name = "Books & Stationery", IsActive = true },
                new BusinessSubCategory { Id = 4, CategoryId = 1, Key = "gifts", Name = "Gifts & Souvenirs", IsActive = true },
                new BusinessSubCategory { Id = 5, CategoryId = 1, Key = "hardware", Name = "Hardware & Tools", IsActive = true },
                new BusinessSubCategory { Id = 6, CategoryId = 1, Key = "pharmacy", Name = "Pharmacy & Health", IsActive = true },
                new BusinessSubCategory { Id = 24, CategoryId = 1, Key = "antique-shop", Name = "Antique Shop", IsActive = true },
                // restaurants-food
                new BusinessSubCategory { Id = 7, CategoryId = 2, Key = "restaurant", Name = "Restaurant", IsActive = true },
                new BusinessSubCategory { Id = 8, CategoryId = 2, Key = "cafe", Name = "Cafe & Coffee Shop", IsActive = true },
                new BusinessSubCategory { Id = 9, CategoryId = 2, Key = "fast-food", Name = "Fast Food", IsActive = true },
                new BusinessSubCategory { Id = 10, CategoryId = 2, Key = "bakery", Name = "Bakery", IsActive = true },
                new BusinessSubCategory { Id = 11, CategoryId = 2, Key = "bar", Name = "Bar & Pub", IsActive = true },
                new BusinessSubCategory { Id = 12, CategoryId = 2, Key = "takeaway", Name = "Takeaway", IsActive = true },
                // markets-vendors
                new BusinessSubCategory { Id = 19, CategoryId = 3, Key = "farmers", Name = "Farmers Market", IsActive = true },
                new BusinessSubCategory { Id = 20, CategoryId = 3, Key = "craft", Name = "Craft Market", IsActive = true },
                new BusinessSubCategory { Id = 21, CategoryId = 3, Key = "flea", Name = "Flea Market", IsActive = true },
                new BusinessSubCategory { Id = 22, CategoryId = 3, Key = "food", Name = "Food Market", IsActive = true },
                new BusinessSubCategory { Id = 23, CategoryId = 3, Key = "antique", Name = "Antique Market", IsActive = true },
                // accommodation
                new BusinessSubCategory { Id = 13, CategoryId = 4, Key = "hotel", Name = "Hotel", IsActive = true },
                new BusinessSubCategory { Id = 14, CategoryId = 4, Key = "guesthouse", Name = "Guesthouse", IsActive = true },
                new BusinessSubCategory { Id = 15, CategoryId = 4, Key = "bnb", Name = "Bed & Breakfast", IsActive = true },
                new BusinessSubCategory { Id = 16, CategoryId = 4, Key = "self-catering", Name = "Self-catering", IsActive = true },
                new BusinessSubCategory { Id = 17, CategoryId = 4, Key = "backpackers", Name = "Backpackers", IsActive = true },
                new BusinessSubCategory { Id = 18, CategoryId = 4, Key = "camping", Name = "Camping & Caravan", IsActive = true }
            );

            builder.Entity<ServiceDefinition>().HasData(
                new ServiceDefinition { Id = 1, Key = "delivery", Name = "Delivery Available", IsActive = true },
                new ServiceDefinition { Id = 2, Key = "takeaway", Name = "Takeaway/Collection", IsActive = true },
                new ServiceDefinition { Id = 3, Key = "wheelchair", Name = "Wheelchair Accessible", IsActive = true },
                new ServiceDefinition { Id = 4, Key = "parking", Name = "Parking Available", IsActive = true },
                new ServiceDefinition { Id = 5, Key = "wifi", Name = "Free WiFi", IsActive = true },
                new ServiceDefinition { Id = 6, Key = "cards", Name = "Card Payments Accepted", IsActive = true }
            );

            // Seed admin message topics
            builder.Entity<AdminMessageTopic>().HasData(
                // High Priority Topics
                new AdminMessageTopic { Id = 1, Key = "payment-issues", Name = "Payment Issues", Description = "Billing problems, payment failures, subscription issues", IconClass = "fas fa-credit-card", Priority = "High", ColorClass = "danger", SortOrder = 1, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 2, Key = "technical-problems", Name = "Technical Problems", Description = "Site bugs, login issues, functionality not working", IconClass = "fas fa-bug", Priority = "High", ColorClass = "danger", SortOrder = 2, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 3, Key = "account-access", Name = "Account Access", Description = "Password resets, account lockouts, permission issues", IconClass = "fas fa-user-lock", Priority = "High", ColorClass = "warning", SortOrder = 3, CreatedAt = DateTime.UtcNow },
                
                // Medium Priority Topics
                new AdminMessageTopic { Id = 4, Key = "feature-requests", Name = "Feature Requests", Description = "New functionality suggestions, improvements", IconClass = "fas fa-lightbulb", Priority = "Medium", ColorClass = "info", SortOrder = 4, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 5, Key = "business-listing", Name = "Business Listing Issues", Description = "Problems with business information, images, approval delays", IconClass = "fas fa-store", Priority = "Medium", ColorClass = "info", SortOrder = 5, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 6, Key = "subscription-changes", Name = "Subscription Changes", Description = "Upgrade/downgrade requests, plan modifications", IconClass = "fas fa-exchange-alt", Priority = "Medium", ColorClass = "info", SortOrder = 6, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 7, Key = "data-corrections", Name = "Data Corrections", Description = "Incorrect town information, business details", IconClass = "fas fa-edit", Priority = "Medium", ColorClass = "warning", SortOrder = 7, CreatedAt = DateTime.UtcNow },
                
                // Low Priority Topics
                new AdminMessageTopic { Id = 8, Key = "general-support", Name = "General Support", Description = "How-to questions, usage guidance", IconClass = "fas fa-question-circle", Priority = "Low", ColorClass = "secondary", SortOrder = 8, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 9, Key = "feedback", Name = "Feedback & Suggestions", Description = "General feedback about the platform", IconClass = "fas fa-comment", Priority = "Low", ColorClass = "secondary", SortOrder = 9, CreatedAt = DateTime.UtcNow },
                new AdminMessageTopic { Id = 10, Key = "partnership-inquiries", Name = "Partnership Inquiries", Description = "Business partnerships, collaboration requests", IconClass = "fas fa-handshake", Priority = "Low", ColorClass = "secondary", SortOrder = 10, CreatedAt = DateTime.UtcNow }
            );
        }

        private void ConfigureCategorySpecificEntities(ModelBuilder builder)
        {
            // Configure MarketDetails
            builder.Entity<MarketDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<MarketDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TourDetails
            builder.Entity<TourDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<TourDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure EventDetails
            builder.Entity<EventDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<EventDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure RestaurantDetails
            builder.Entity<RestaurantDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<RestaurantDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure AccommodationDetails
            builder.Entity<AccommodationDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<AccommodationDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ShopDetails
            builder.Entity<ShopDetails>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithOne()
                      .HasForeignKey<ShopDetails>(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BusinessAlert
            builder.Entity<BusinessAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SpecialOperatingHours
            builder.Entity<SpecialOperatingHours>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure BusinessCategory
            builder.Entity<BusinessCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconClass).HasMaxLength(100);
                entity.HasIndex(e => e.Key).IsUnique();
            });

            // Configure BusinessSubCategory
            builder.Entity<BusinessSubCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => new { e.CategoryId, e.Key }).IsUnique();
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ServiceDefinition
            builder.Entity<ServiceDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Key).IsUnique();
            });
        }

        private void ConfigureMemberEntities(ModelBuilder builder)
        {
            // Configure BusinessReview
            builder.Entity<BusinessReview>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one review per user per business
                entity.HasIndex(e => new { e.BusinessId, e.UserId }).IsUnique();
                
                // Indexes for analytics queries
                entity.HasIndex(e => new { e.BusinessId, e.CreatedAt }).HasDatabaseName("IX_BusinessReviews_BusinessId_CreatedAt");
                entity.HasIndex(e => new { e.BusinessId, e.IsActive, e.CreatedAt }).HasDatabaseName("IX_BusinessReviews_BusinessId_IsActive_CreatedAt");
            });

            // Configure BusinessReviewResponse
            builder.Entity<BusinessReviewResponse>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Response).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.BusinessReview)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessReviewId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Unique constraint: one response per review (business owner can only respond once)
                entity.HasIndex(e => e.BusinessReviewId).IsUnique();
            });

            // Configure FavoriteBusiness
            builder.Entity<FavoriteBusiness>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint: one favorite per user per business
                entity.HasIndex(e => new { e.BusinessId, e.UserId }).IsUnique();
                
                // Indexes for analytics queries
                entity.HasIndex(e => new { e.BusinessId, e.CreatedAt }).HasDatabaseName("IX_FavoriteBusinesses_BusinessId_CreatedAt");
            });
        }

        private void ConfigureTrialEntities(ModelBuilder builder)
        {
            // Configure TrialAuditLog
            builder.Entity<TrialAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.Timestamp);
            });
        }

        private void ConfigureErrorLoggingEntities(ModelBuilder builder)
        {
            // Configure ErrorLogEntry
            builder.Entity<ErrorLogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ErrorType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.RequestPath).HasMaxLength(500);
                entity.Property(e => e.UserAgent).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ResolvedBy).HasMaxLength(450);

                // Foreign key relationships
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ResolvedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ResolvedBy)
                      .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_ErrorLogs_Timestamp");
                entity.HasIndex(e => e.ErrorType).HasDatabaseName("IX_ErrorLogs_ErrorType");
                entity.HasIndex(e => e.Severity).HasDatabaseName("IX_ErrorLogs_Severity");
                entity.HasIndex(e => e.IsResolved).HasDatabaseName("IX_ErrorLogs_IsResolved");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_ErrorLogs_UserId");
                entity.HasIndex(e => new { e.Timestamp, e.Severity }).HasDatabaseName("IX_ErrorLogs_Timestamp_Severity");
            });

            // Configure AnalyticsSnapshot
            builder.Entity<AnalyticsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SnapshotDate).HasColumnType("date");
                entity.Property(e => e.TotalViews).HasDefaultValue(0);
                entity.Property(e => e.TotalReviews).HasDefaultValue(0);
                entity.Property(e => e.TotalFavorites).HasDefaultValue(0);
                entity.Property(e => e.AverageRating).HasColumnType("decimal(3,2)");
                entity.Property(e => e.EngagementScore).HasColumnType("decimal(5,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationship
                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Unique constraint to prevent duplicate snapshots for the same business and date
                entity.HasIndex(e => new { e.BusinessId, e.SnapshotDate }).IsUnique();
                
                // Indexes for performance
                entity.HasIndex(e => e.SnapshotDate).HasDatabaseName("IX_AnalyticsSnapshots_SnapshotDate");
                entity.HasIndex(e => e.BusinessId).HasDatabaseName("IX_AnalyticsSnapshots_BusinessId");
            });

        }



        private void ConfigureAdminMessageEntities(ModelBuilder builder)
        {
            // Configure AdminMessageTopic
            builder.Entity<AdminMessageTopic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Key).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconClass).HasMaxLength(100);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ColorClass).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(e => e.Key).IsUnique();
                entity.HasIndex(e => e.SortOrder);
            });

            // Configure AdminMessage
            builder.Entity<AdminMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ResolvedBy).HasMaxLength(450);
                entity.Property(e => e.AdminResponse).HasMaxLength(2000);
                entity.Property(e => e.ResponseBy).HasMaxLength(450);

                // Foreign key relationships
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Topic)
                      .WithMany(t => t.Messages)
                      .HasForeignKey(e => e.TopicId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ResolvedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ResolvedBy)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ResponseByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ResponseBy)
                      .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.TopicId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => new { e.Status, e.Priority });
            });
        }

        private void ConfigureAnalyticsAuditLogEntity(ModelBuilder builder)
        {
            // Configure AnalyticsAuditLog
            builder.Entity<AnalyticsAuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.BusinessId).HasMaxLength(50);
                entity.Property(e => e.Platform).HasMaxLength(20);
                entity.Property(e => e.ExportType).HasMaxLength(50);
                entity.Property(e => e.Format).HasMaxLength(20);
                entity.Property(e => e.Details).HasMaxLength(1000);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsSuspicious).HasDefaultValue(false);

                // Foreign key relationship to ApplicationUser
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance and querying
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsAuditLogs_UserId");
                entity.HasIndex(e => e.Action).HasDatabaseName("IX_AnalyticsAuditLogs_Action");
                entity.HasIndex(e => e.Timestamp).HasDatabaseName("IX_AnalyticsAuditLogs_Timestamp");
                entity.HasIndex(e => e.IsSuspicious).HasDatabaseName("IX_AnalyticsAuditLogs_IsSuspicious");
                entity.HasIndex(e => e.IpAddress).HasDatabaseName("IX_AnalyticsAuditLogs_IpAddress");
                entity.HasIndex(e => new { e.UserId, e.Timestamp }).HasDatabaseName("IX_AnalyticsAuditLogs_UserId_Timestamp");
                entity.HasIndex(e => new { e.IsSuspicious, e.Timestamp }).HasDatabaseName("IX_AnalyticsAuditLogs_Suspicious_Timestamp");
            });
        }

        private void ConfigureAnalyticsMonitoringEntities(ModelBuilder builder)
        {
            // Configure AnalyticsPerformanceLog
            builder.Entity<AnalyticsPerformanceLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MetricName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.IsSuccess).HasDefaultValue(true);
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.Platform).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.Context).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationship to ApplicationUser
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance and querying
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsPerformanceLogs_UserId");
                entity.HasIndex(e => e.MetricType).HasDatabaseName("IX_AnalyticsPerformanceLogs_MetricType");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AnalyticsPerformanceLogs_CreatedAt");
                entity.HasIndex(e => e.IsSuccess).HasDatabaseName("IX_AnalyticsPerformanceLogs_IsSuccess");
                entity.HasIndex(e => new { e.MetricType, e.CreatedAt }).HasDatabaseName("IX_AnalyticsPerformanceLogs_MetricType_CreatedAt");
                entity.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("IX_AnalyticsPerformanceLogs_UserId_CreatedAt");
            });

            // Configure AnalyticsErrorLog
            builder.Entity<AnalyticsErrorLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ErrorType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorCategory).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(500);
                entity.Property(e => e.StackTrace).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Context).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.IsResolved).HasDefaultValue(false);
                entity.Property(e => e.ResolvedBy).HasMaxLength(450);
                entity.Property(e => e.Platform).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.ResolvedBy)
                      .OnDelete(DeleteBehavior.NoAction);

                // Indexes for performance and querying
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsErrorLogs_UserId");
                entity.HasIndex(e => e.ErrorType).HasDatabaseName("IX_AnalyticsErrorLogs_ErrorType");
                entity.HasIndex(e => e.Severity).HasDatabaseName("IX_AnalyticsErrorLogs_Severity");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AnalyticsErrorLogs_CreatedAt");
                entity.HasIndex(e => e.IsResolved).HasDatabaseName("IX_AnalyticsErrorLogs_IsResolved");
                entity.HasIndex(e => new { e.ErrorType, e.CreatedAt }).HasDatabaseName("IX_AnalyticsErrorLogs_ErrorType_CreatedAt");
                entity.HasIndex(e => new { e.Severity, e.IsResolved }).HasDatabaseName("IX_AnalyticsErrorLogs_Severity_Resolved");
            });

            // Configure AnalyticsUsageLog
            builder.Entity<AnalyticsUsageLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.UsageType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FeatureName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Duration);
                entity.Property(e => e.InteractionType).HasMaxLength(100);
                entity.Property(e => e.Metadata).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Platform).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationship to ApplicationUser
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance and querying
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsUsageLogs_UserId");
                entity.HasIndex(e => e.UsageType).HasDatabaseName("IX_AnalyticsUsageLogs_UsageType");
                entity.HasIndex(e => e.FeatureName).HasDatabaseName("IX_AnalyticsUsageLogs_FeatureName");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AnalyticsUsageLogs_CreatedAt");
                entity.HasIndex(e => e.SessionId).HasDatabaseName("IX_AnalyticsUsageLogs_SessionId");
                entity.HasIndex(e => new { e.UsageType, e.CreatedAt }).HasDatabaseName("IX_AnalyticsUsageLogs_UsageType_CreatedAt");
                entity.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("IX_AnalyticsUsageLogs_UserId_CreatedAt");
            });

            // Configure AnalyticsShareableLink
            builder.Entity<AnalyticsShareableLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.LinkToken).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DashboardType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance and querying
                entity.HasIndex(e => e.LinkToken).IsUnique().HasDatabaseName("IX_AnalyticsShareableLinks_LinkToken");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsShareableLinks_UserId");
                entity.HasIndex(e => e.DashboardType).HasDatabaseName("IX_AnalyticsShareableLinks_DashboardType");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AnalyticsShareableLinks_CreatedAt");
                entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("IX_AnalyticsShareableLinks_ExpiresAt");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_AnalyticsShareableLinks_IsActive");
                entity.HasIndex(e => new { e.LinkToken, e.IsActive }).HasDatabaseName("IX_AnalyticsShareableLinks_Token_Active");
            });

            // Configure AnalyticsEmailReport
            builder.Entity<AnalyticsEmailReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.ReportType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Frequency).IsRequired().HasMaxLength(20);
                entity.Property(e => e.EmailAddress).HasMaxLength(500);
                entity.Property(e => e.CustomMessage).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Indexes for performance and querying
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsEmailReports_UserId");
                entity.HasIndex(e => e.ReportType).HasDatabaseName("IX_AnalyticsEmailReports_ReportType");
                entity.HasIndex(e => e.Frequency).HasDatabaseName("IX_AnalyticsEmailReports_Frequency");
                entity.HasIndex(e => e.CreatedAt).HasDatabaseName("IX_AnalyticsEmailReports_CreatedAt");
                entity.HasIndex(e => e.NextScheduledAt).HasDatabaseName("IX_AnalyticsEmailReports_NextScheduled");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_AnalyticsEmailReports_IsActive");
                entity.HasIndex(e => new { e.UserId, e.IsActive }).HasDatabaseName("IX_AnalyticsEmailReports_UserId_Active");
            });
        }

        private void ConfigureAnalyticsEntities(ModelBuilder builder)
        {
            // Configure BusinessViewLog entity
            builder.Entity<BusinessViewLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BusinessId).IsRequired();
                entity.Property(e => e.Platform).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ViewedAt).IsRequired();
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.Referrer).HasMaxLength(500);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(450);
                
                // Foreign key relationships
                entity.HasOne(e => e.Business)
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes for analytics queries
                entity.HasIndex(e => new { e.BusinessId, e.ViewedAt, e.Platform }).HasDatabaseName("IX_BusinessViewLogs_BusinessId_ViewedAt_Platform");
                entity.HasIndex(e => new { e.UserId, e.ViewedAt }).HasDatabaseName("IX_BusinessViewLogs_UserId_ViewedAt");
                entity.HasIndex(e => new { e.Platform, e.ViewedAt }).HasDatabaseName("IX_BusinessViewLogs_Platform_ViewedAt");
                entity.HasIndex(e => e.ViewedAt).HasDatabaseName("IX_BusinessViewLogs_ViewedAt");
                entity.HasIndex(e => e.BusinessId).HasDatabaseName("IX_BusinessViewLogs_BusinessId");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_BusinessViewLogs_UserId");
                entity.HasIndex(e => e.Platform).HasDatabaseName("IX_BusinessViewLogs_Platform");
            });

            // Configure AnalyticsEvents entity
            builder.Entity<AnalyticsEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OccurredAt).IsRequired();
                entity.Property(e => e.EventData).HasColumnType("nvarchar(max)");
                entity.Property(e => e.UserId).HasMaxLength(450);
                entity.Property(e => e.BusinessId);
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes for event sourcing queries
                entity.HasIndex(e => new { e.EventType, e.OccurredAt }).HasDatabaseName("IX_AnalyticsEvents_EventType_OccurredAt");
                entity.HasIndex(e => new { e.UserId, e.EventType, e.OccurredAt }).HasDatabaseName("IX_AnalyticsEvents_UserId_EventType_OccurredAt");
                entity.HasIndex(e => new { e.BusinessId, e.EventType, e.OccurredAt }).HasDatabaseName("IX_AnalyticsEvents_BusinessId_EventType_OccurredAt");
                entity.HasIndex(e => e.OccurredAt).HasDatabaseName("IX_AnalyticsEvents_OccurredAt");
                entity.HasIndex(e => e.EventType).HasDatabaseName("IX_AnalyticsEvents_EventType");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnalyticsEvents_UserId");
                entity.HasIndex(e => e.BusinessId).HasDatabaseName("IX_AnalyticsEvents_BusinessId");
            });

            // Configure CustomMetric entity
            builder.Entity<CustomMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                
                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes for custom metrics
                entity.HasIndex(e => new { e.UserId, e.IsActive }).HasDatabaseName("IX_CustomMetrics_UserId_IsActive");
                entity.HasIndex(e => new { e.Category, e.IsActive }).HasDatabaseName("IX_CustomMetrics_Category_IsActive");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_CustomMetrics_UserId");
                entity.HasIndex(e => e.Category).HasDatabaseName("IX_CustomMetrics_Category");
                entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_CustomMetrics_IsActive");
            });

            // Configure CustomMetricDataPoint entity
            builder.Entity<CustomMetricDataPoint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomMetricId).IsRequired();
                entity.Property(e => e.Value).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Context).HasColumnType("nvarchar(max)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key relationships
                entity.HasOne(e => e.CustomMetric)
                      .WithMany(m => m.HistoricalData)
                      .HasForeignKey(e => e.CustomMetricId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes for data point queries
                entity.HasIndex(e => new { e.CustomMetricId, e.Date }).HasDatabaseName("IX_CustomMetricDataPoints_CustomMetricId_Date");
                entity.HasIndex(e => e.Date).HasDatabaseName("IX_CustomMetricDataPoints_Date");
                entity.HasIndex(e => e.CustomMetricId).HasDatabaseName("IX_CustomMetricDataPoints_CustomMetricId");
            });

            // Configure CustomMetricGoal entity
            builder.Entity<CustomMetricGoal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomMetricId).IsRequired();
                entity.Property(e => e.TargetValue).IsRequired();
                entity.Property(e => e.TargetDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key relationships
                entity.HasOne(e => e.CustomMetric)
                      .WithMany(m => m.Goals)
                      .HasForeignKey(e => e.CustomMetricId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes for goal queries
                entity.HasIndex(e => new { e.CustomMetricId, e.Status }).HasDatabaseName("IX_CustomMetricGoals_CustomMetricId_Status");
                entity.HasIndex(e => e.CustomMetricId).HasDatabaseName("IX_CustomMetricGoals_CustomMetricId");
                entity.HasIndex(e => e.Status).HasDatabaseName("IX_CustomMetricGoals_Status");
            });

            // Configure AnomalyDetection entity
            builder.Entity<AnomalyDetection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.BusinessId);
                entity.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.ActualValue).IsRequired();
                entity.Property(e => e.ExpectedValue).IsRequired();
                entity.Property(e => e.Deviation).IsRequired();
                entity.Property(e => e.DeviationPercentage).IsRequired();
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Context).HasMaxLength(1000);
                entity.Property(e => e.PossibleCauses).HasColumnType("nvarchar(max)");
                entity.Property(e => e.Recommendations).HasColumnType("nvarchar(max)");
                entity.Property(e => e.IsAcknowledged).HasDefaultValue(false);
                entity.Property(e => e.AcknowledgedBy).HasMaxLength(450);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.AcknowledgedBy)
                      .OnDelete(DeleteBehavior.NoAction);
                
                // Indexes for anomaly detection
                entity.HasIndex(e => new { e.UserId, e.Date }).HasDatabaseName("IX_AnomalyDetections_UserId_Date");
                entity.HasIndex(e => new { e.MetricType, e.Severity }).HasDatabaseName("IX_AnomalyDetections_MetricType_Severity");
                entity.HasIndex(e => e.IsAcknowledged).HasDatabaseName("IX_AnomalyDetections_IsAcknowledged");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_AnomalyDetections_UserId");
                entity.HasIndex(e => e.Date).HasDatabaseName("IX_AnomalyDetections_Date");
            });

            // Configure PredictiveForecast entity
            builder.Entity<PredictiveForecast>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.BusinessId);
                entity.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ForecastDate).IsRequired();
                entity.Property(e => e.PredictedValue).IsRequired();
                entity.Property(e => e.LowerBound).IsRequired();
                entity.Property(e => e.UpperBound).IsRequired();
                entity.Property(e => e.Confidence).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes for predictive forecasts
                entity.HasIndex(e => new { e.UserId, e.ForecastDate }).HasDatabaseName("IX_PredictiveForecasts_UserId_ForecastDate");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_PredictiveForecasts_UserId");
                entity.HasIndex(e => e.ForecastDate).HasDatabaseName("IX_PredictiveForecasts_ForecastDate");
            });

            // Configure SeasonalPattern entity
            builder.Entity<SeasonalPattern>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
                entity.Property(e => e.BusinessId);
                entity.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PatternType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Period).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AverageValue).IsRequired();
                entity.Property(e => e.Deviation).IsRequired();
                entity.Property(e => e.Strength).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Foreign key relationships
                entity.HasOne<ApplicationUser>()
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne<Business>()
                      .WithMany()
                      .HasForeignKey(e => e.BusinessId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes for seasonal patterns
                entity.HasIndex(e => new { e.UserId, e.PatternType }).HasDatabaseName("IX_SeasonalPatterns_UserId_PatternType");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_SeasonalPatterns_UserId");
                entity.HasIndex(e => e.PatternType).HasDatabaseName("IX_SeasonalPatterns_PatternType");
            });
        }
    }
}