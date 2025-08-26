using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class CheckForChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId",
                table: "AnomalyDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessViewLogs_AspNetUsers_UserId",
                table: "BusinessViewLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId",
                table: "PredictiveForecasts");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId",
                table: "SeasonalPatterns");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeasonalPatterns",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "SeasonalPatterns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "SeasonalPatterns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PredictiveForecasts",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "PredictiveForecasts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "PredictiveForecasts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomMetrics",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CustomMetrics",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetrics",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "CustomMetrics",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetricGoals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetricDataPoints",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAcknowledged",
                table: "AnomalyDetections",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AnomalyDetections",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgedByUserId",
                table: "AnomalyDetections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "AnomalyDetections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "AnomalyDetections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "AnalyticsEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "AnalyticsEvents",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3860));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3862));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3863));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3865));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3867));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3868));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3870));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3872));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3873));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3875));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3304));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3306));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 26, 11, 3, 13, 359, DateTimeKind.Utc).AddTicks(3308));

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_BusinessId1",
                table: "SeasonalPatterns",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_PatternType",
                table: "SeasonalPatterns",
                column: "PatternType");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_UserId_PatternType",
                table: "SeasonalPatterns",
                columns: new[] { "UserId", "PatternType" });

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalPatterns_UserId1",
                table: "SeasonalPatterns",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_BusinessId1",
                table: "PredictiveForecasts",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_ForecastDate",
                table: "PredictiveForecasts",
                column: "ForecastDate");

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_UserId_ForecastDate",
                table: "PredictiveForecasts",
                columns: new[] { "UserId", "ForecastDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PredictiveForecasts_UserId1",
                table: "PredictiveForecasts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_Category",
                table: "CustomMetrics",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_Category_IsActive",
                table: "CustomMetrics",
                columns: new[] { "Category", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_IsActive",
                table: "CustomMetrics",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_UserId_IsActive",
                table: "CustomMetrics",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetrics_UserId1",
                table: "CustomMetrics",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricGoals_CustomMetricId_Status",
                table: "CustomMetricGoals",
                columns: new[] { "CustomMetricId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricGoals_Status",
                table: "CustomMetricGoals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricDataPoints_CustomMetricId_Date",
                table: "CustomMetricDataPoints",
                columns: new[] { "CustomMetricId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomMetricDataPoints_Date",
                table: "CustomMetricDataPoints",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessViewLogs_BusinessId_ViewedAt_Platform",
                table: "BusinessViewLogs",
                columns: new[] { "BusinessId", "ViewedAt", "Platform" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessViewLogs_Platform",
                table: "BusinessViewLogs",
                column: "Platform");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessViewLogs_Platform_ViewedAt",
                table: "BusinessViewLogs",
                columns: new[] { "Platform", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessViewLogs_UserId_ViewedAt",
                table: "BusinessViewLogs",
                columns: new[] { "UserId", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessViewLogs_ViewedAt",
                table: "BusinessViewLogs",
                column: "ViewedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_AcknowledgedByUserId",
                table: "AnomalyDetections",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_BusinessId1",
                table: "AnomalyDetections",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_Date",
                table: "AnomalyDetections",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_IsAcknowledged",
                table: "AnomalyDetections",
                column: "IsAcknowledged");

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_MetricType_Severity",
                table: "AnomalyDetections",
                columns: new[] { "MetricType", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_UserId_Date",
                table: "AnomalyDetections",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_AnomalyDetections_UserId1",
                table: "AnomalyDetections",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_BusinessId_EventType_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "BusinessId", "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_BusinessId1",
                table: "AnalyticsEvents",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_OccurredAt",
                table: "AnalyticsEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId_EventType_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "UserId", "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId1",
                table: "AnalyticsEvents",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId",
                table: "AnalyticsEvents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId1",
                table: "AnalyticsEvents",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId",
                table: "AnalyticsEvents",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId1",
                table: "AnalyticsEvents",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnomalyDetections_AspNetUsers_AcknowledgedByUserId",
                table: "AnomalyDetections",
                column: "AcknowledgedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnomalyDetections_AspNetUsers_UserId1",
                table: "AnomalyDetections",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId",
                table: "AnomalyDetections",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId1",
                table: "AnomalyDetections",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessViewLogs_AspNetUsers_UserId",
                table: "BusinessViewLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomMetrics_AspNetUsers_UserId1",
                table: "CustomMetrics",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PredictiveForecasts_AspNetUsers_UserId1",
                table: "PredictiveForecasts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId",
                table: "PredictiveForecasts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId1",
                table: "PredictiveForecasts",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPatterns_AspNetUsers_UserId1",
                table: "SeasonalPatterns",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId",
                table: "SeasonalPatterns",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId1",
                table: "SeasonalPatterns",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId1",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId1",
                table: "AnalyticsEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_AnomalyDetections_AspNetUsers_AcknowledgedByUserId",
                table: "AnomalyDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_AnomalyDetections_AspNetUsers_UserId1",
                table: "AnomalyDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId",
                table: "AnomalyDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId1",
                table: "AnomalyDetections");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessViewLogs_AspNetUsers_UserId",
                table: "BusinessViewLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomMetrics_AspNetUsers_UserId1",
                table: "CustomMetrics");

            migrationBuilder.DropForeignKey(
                name: "FK_PredictiveForecasts_AspNetUsers_UserId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId",
                table: "PredictiveForecasts");

            migrationBuilder.DropForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPatterns_AspNetUsers_UserId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId",
                table: "SeasonalPatterns");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropIndex(
                name: "IX_SeasonalPatterns_BusinessId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropIndex(
                name: "IX_SeasonalPatterns_PatternType",
                table: "SeasonalPatterns");

            migrationBuilder.DropIndex(
                name: "IX_SeasonalPatterns_UserId_PatternType",
                table: "SeasonalPatterns");

            migrationBuilder.DropIndex(
                name: "IX_SeasonalPatterns_UserId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropIndex(
                name: "IX_PredictiveForecasts_BusinessId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropIndex(
                name: "IX_PredictiveForecasts_ForecastDate",
                table: "PredictiveForecasts");

            migrationBuilder.DropIndex(
                name: "IX_PredictiveForecasts_UserId_ForecastDate",
                table: "PredictiveForecasts");

            migrationBuilder.DropIndex(
                name: "IX_PredictiveForecasts_UserId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetrics_Category",
                table: "CustomMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetrics_Category_IsActive",
                table: "CustomMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetrics_IsActive",
                table: "CustomMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetrics_UserId_IsActive",
                table: "CustomMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetrics_UserId1",
                table: "CustomMetrics");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetricGoals_CustomMetricId_Status",
                table: "CustomMetricGoals");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetricGoals_Status",
                table: "CustomMetricGoals");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetricDataPoints_CustomMetricId_Date",
                table: "CustomMetricDataPoints");

            migrationBuilder.DropIndex(
                name: "IX_CustomMetricDataPoints_Date",
                table: "CustomMetricDataPoints");

            migrationBuilder.DropIndex(
                name: "IX_BusinessViewLogs_BusinessId_ViewedAt_Platform",
                table: "BusinessViewLogs");

            migrationBuilder.DropIndex(
                name: "IX_BusinessViewLogs_Platform",
                table: "BusinessViewLogs");

            migrationBuilder.DropIndex(
                name: "IX_BusinessViewLogs_Platform_ViewedAt",
                table: "BusinessViewLogs");

            migrationBuilder.DropIndex(
                name: "IX_BusinessViewLogs_UserId_ViewedAt",
                table: "BusinessViewLogs");

            migrationBuilder.DropIndex(
                name: "IX_BusinessViewLogs_ViewedAt",
                table: "BusinessViewLogs");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_AcknowledgedByUserId",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_BusinessId1",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_Date",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_IsAcknowledged",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_MetricType_Severity",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_UserId_Date",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnomalyDetections_UserId1",
                table: "AnomalyDetections");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_BusinessId_EventType_OccurredAt",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_BusinessId1",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_EventType",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_EventType_OccurredAt",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_OccurredAt",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_UserId_EventType_OccurredAt",
                table: "AnalyticsEvents");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsEvents_UserId1",
                table: "AnalyticsEvents");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "SeasonalPatterns");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PredictiveForecasts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "CustomMetrics");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByUserId",
                table: "AnomalyDetections");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "AnomalyDetections");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AnomalyDetections");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "AnalyticsEvents");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "AnalyticsEvents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SeasonalPatterns",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "PredictiveForecasts",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CustomMetrics",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "CustomMetrics",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetrics",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetricGoals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CustomMetricDataPoints",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAcknowledged",
                table: "AnomalyDetections",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "AnomalyDetections",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(714));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(716));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(718));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(719));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(721));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(723));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(724));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(726));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(728));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(729));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(129));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(131));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 20, 42, 32, 735, DateTimeKind.Utc).AddTicks(133));

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_AspNetUsers_UserId",
                table: "AnalyticsEvents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsEvents_Businesses_BusinessId",
                table: "AnalyticsEvents",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnomalyDetections_Businesses_BusinessId",
                table: "AnomalyDetections",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessViewLogs_AspNetUsers_UserId",
                table: "BusinessViewLogs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PredictiveForecasts_Businesses_BusinessId",
                table: "PredictiveForecasts",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalPatterns_Businesses_BusinessId",
                table: "SeasonalPatterns",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id");
        }
    }
}
