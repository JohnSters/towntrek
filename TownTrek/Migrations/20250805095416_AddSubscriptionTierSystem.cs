using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionTierSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Businesses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthenticationMethod",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CurrentSubscriptionTier",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacebookId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasActiveSubscription",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionStartDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SubscriptionTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedById = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTiers_AspNetUsers_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PriceChangeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionTierId = table.Column<int>(type: "int", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotificationSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceChangeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceChangeHistory_AspNetUsers_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceChangeHistory_SubscriptionTiers_SubscriptionTierId",
                        column: x => x.SubscriptionTierId,
                        principalTable: "SubscriptionTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubscriptionTierId = table.Column<int>(type: "int", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PayFastToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayFastPaymentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionTiers_SubscriptionTierId",
                        column: x => x.SubscriptionTierId,
                        principalTable: "SubscriptionTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTierFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionTierId = table.Column<int>(type: "int", nullable: false),
                    FeatureKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTierFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTierFeatures_SubscriptionTiers_SubscriptionTierId",
                        column: x => x.SubscriptionTierId,
                        principalTable: "SubscriptionTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionTierLimits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionTierId = table.Column<int>(type: "int", nullable: false),
                    LimitType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LimitValue = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionTierLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionTierLimits_SubscriptionTiers_SubscriptionTierId",
                        column: x => x.SubscriptionTierId,
                        principalTable: "SubscriptionTiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "SubscriptionTiers",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayName", "IsActive", "MonthlyPrice", "Name", "SortOrder", "UpdatedAt", "UpdatedById" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8096), "Perfect for small businesses getting started", "Basic Plan", true, 199.00m, "Basic", 1, null, null },
                    { 2, new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8098), "Great for growing businesses with multiple locations", "Standard Plan", true, 399.00m, "Standard", 2, null, null },
                    { 3, new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8100), "Full-featured plan for established businesses", "Premium Plan", true, 599.00m, "Premium", 3, null, null }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionTierFeatures",
                columns: new[] { "Id", "Description", "FeatureKey", "FeatureName", "IsEnabled", "SubscriptionTierId" },
                values: new object[,]
                {
                    { 1, null, "BasicSupport", "Standard Support", true, 1 },
                    { 2, null, "BasicSupport", "Standard Support", true, 2 },
                    { 3, null, "PrioritySupport", "Priority Support", true, 2 },
                    { 4, null, "BasicAnalytics", "Basic Analytics", true, 2 },
                    { 5, null, "PDFUploads", "PDF Document Uploads", true, 2 },
                    { 6, null, "BasicSupport", "Standard Support", true, 3 },
                    { 7, null, "PrioritySupport", "Priority Support", true, 3 },
                    { 8, null, "DedicatedSupport", "Dedicated Support", true, 3 },
                    { 9, null, "AdvancedAnalytics", "Advanced Analytics", true, 3 },
                    { 10, null, "FeaturedPlacement", "Featured Placement", true, 3 },
                    { 11, null, "PDFUploads", "PDF Document Uploads", true, 3 }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionTierLimits",
                columns: new[] { "Id", "Description", "LimitType", "LimitValue", "SubscriptionTierId" },
                values: new object[,]
                {
                    { 1, null, "MaxBusinesses", 1, 1 },
                    { 2, null, "MaxImages", 5, 1 },
                    { 3, null, "MaxPDFs", 0, 1 },
                    { 4, null, "MaxBusinesses", 3, 2 },
                    { 5, null, "MaxImages", 15, 2 },
                    { 6, null, "MaxPDFs", 5, 2 },
                    { 7, null, "MaxBusinesses", 10, 3 },
                    { 8, null, "MaxImages", -1, 3 },
                    { 9, null, "MaxPDFs", -1, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_ApplicationUserId",
                table: "Businesses",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeHistory_ChangedById",
                table: "PriceChangeHistory",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_PriceChangeHistory_SubscriptionTierId",
                table: "PriceChangeHistory",
                column: "SubscriptionTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionTierId",
                table: "Subscriptions",
                column: "SubscriptionTierId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTierFeatures_SubscriptionTierId_FeatureKey",
                table: "SubscriptionTierFeatures",
                columns: new[] { "SubscriptionTierId", "FeatureKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTierLimits_SubscriptionTierId_LimitType",
                table: "SubscriptionTierLimits",
                columns: new[] { "SubscriptionTierId", "LimitType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTiers_Name",
                table: "SubscriptionTiers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTiers_SortOrder",
                table: "SubscriptionTiers",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionTiers_UpdatedById",
                table: "SubscriptionTiers",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_AspNetUsers_ApplicationUserId",
                table: "Businesses",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_AspNetUsers_ApplicationUserId",
                table: "Businesses");

            migrationBuilder.DropTable(
                name: "PriceChangeHistory");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SubscriptionTierFeatures");

            migrationBuilder.DropTable(
                name: "SubscriptionTierLimits");

            migrationBuilder.DropTable(
                name: "SubscriptionTiers");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_ApplicationUserId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "AuthenticationMethod",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentSubscriptionTier",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FacebookId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HasActiveSubscription",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionStartDate",
                table: "AspNetUsers");
        }
    }
}
