using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedAnalyticsAndDashboardCustomization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnomalyDetections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    MetricType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualValue = table.Column<double>(type: "float", nullable: false),
                    ExpectedValue = table.Column<double>(type: "float", nullable: false),
                    Deviation = table.Column<double>(type: "float", nullable: false),
                    DeviationPercentage = table.Column<double>(type: "float", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Context = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PossibleCauses = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnomalyDetections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnomalyDetections_AspNetUsers_AcknowledgedBy",
                        column: x => x.AcknowledgedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnomalyDetections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnomalyDetections_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Formula = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsUserDefined = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastCalculated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentValue = table.Column<double>(type: "float", nullable: false),
                    PreviousValue = table.Column<double>(type: "float", nullable: false),
                    ChangePercentage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomMetrics_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ShowViewsCard = table.Column<bool>(type: "bit", nullable: false),
                    ShowReviewsCard = table.Column<bool>(type: "bit", nullable: false),
                    ShowFavoritesCard = table.Column<bool>(type: "bit", nullable: false),
                    ShowEngagementCard = table.Column<bool>(type: "bit", nullable: false),
                    ShowPerformanceChart = table.Column<bool>(type: "bit", nullable: false),
                    ShowViewsChart = table.Column<bool>(type: "bit", nullable: false),
                    ShowReviewsChart = table.Column<bool>(type: "bit", nullable: false),
                    LayoutType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RefreshInterval = table.Column<int>(type: "int", nullable: false),
                    DefaultDateRange = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FocusedBusinessId = table.Column<int>(type: "int", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DashboardPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardPreferences_Businesses_FocusedBusinessId",
                        column: x => x.FocusedBusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PredictiveForecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    MetricType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ForecastDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PredictedValue = table.Column<double>(type: "float", nullable: false),
                    LowerBound = table.Column<double>(type: "float", nullable: false),
                    UpperBound = table.Column<double>(type: "float", nullable: false),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredictiveForecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PredictiveForecasts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PredictiveForecasts_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SavedDashboardViews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DateRange = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    LayoutType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WidgetConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedDashboardViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedDashboardViews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedDashboardViews_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SeasonalPatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    MetricType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatternType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AverageValue = table.Column<double>(type: "float", nullable: false),
                    Deviation = table.Column<double>(type: "float", nullable: false),
                    Strength = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalPatterns_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonalPatterns_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomMetricDataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomMetricId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMetricDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomMetricDataPoints_CustomMetrics_CustomMetricId",
                        column: x => x.CustomMetricId,
                        principalTable: "CustomMetrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomMetricGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomMetricId = table.Column<int>(type: "int", nullable: false),
                    TargetValue = table.Column<double>(type: "float", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AchievedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMetricGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomMetricGoals_CustomMetrics_CustomMetricId",
                        column: x => x.CustomMetricId,
                        principalTable: "CustomMetrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1041));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1043));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1044));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1046));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1048));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1049));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1051));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1053));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1054));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(1056));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(568));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(570));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 19, 3, 16, 534, DateTimeKind.Utc).AddTicks(572));

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_AcknowledgedBy",
                table: "AnomalyDetections",
                column: "AcknowledgedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_BusinessId",
                table: "AnomalyDetections",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_UserId",
                table: "AnomalyDetections",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricDataPoints_CustomMetricId",
                table: "CustomMetricDataPoints",
                column: "CustomMetricId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricGoals_CustomMetricId",
                table: "CustomMetricGoals",
                column: "CustomMetricId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_UserId",
                table: "CustomMetrics",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardPreferences_FocusedBusinessId",
                table: "DashboardPreferences",
                column: "FocusedBusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardPreferences_UserId",
                table: "DashboardPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_BusinessId",
                table: "PredictiveForecasts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_UserId",
                table: "PredictiveForecasts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedDashboardViews_BusinessId",
                table: "SavedDashboardViews",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedDashboardViews_UserId",
                table: "SavedDashboardViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_BusinessId",
                table: "SeasonalPatterns",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_UserId",
                table: "SeasonalPatterns",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnomalyDetections");

            migrationBuilder.DropTable(
                name: "CustomMetricDataPoints");

            migrationBuilder.DropTable(
                name: "CustomMetricGoals");

            migrationBuilder.DropTable(
                name: "DashboardPreferences");

            migrationBuilder.DropTable(
                name: "PredictiveForecasts");

            migrationBuilder.DropTable(
                name: "SavedDashboardViews");

            migrationBuilder.DropTable(
                name: "SeasonalPatterns");

            migrationBuilder.DropTable(
                name: "CustomMetrics");

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1558));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1560));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1562));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1563));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1565));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1567));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1568));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1570));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1572));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(1574));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(940));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(942));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 16, 29, 53, 224, DateTimeKind.Utc).AddTicks(944));
        }
    }
}
