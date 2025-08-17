using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsMonitoringTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsErrorLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ErrorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErrorCategory = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StackTrace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsErrorLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsErrorLogs_AspNetUsers_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnalyticsErrorLogs_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnalyticsErrorLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsErrorLogs_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsPerformanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MetricName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Context = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsPerformanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsPerformanceLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsPerformanceLogs_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsUsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UsageType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: true),
                    InteractionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsUsageLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsUsageLogs_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1392));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1394));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1396));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1398));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1399));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1401));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1403));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1404));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1406));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(1408));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(911));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(913));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 46, 3, 771, DateTimeKind.Utc).AddTicks(915));

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_CreatedAt",
                table: "AnalyticsErrorLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_ErrorType",
                table: "AnalyticsErrorLogs",
                column: "ErrorType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_ErrorType_CreatedAt",
                table: "AnalyticsErrorLogs",
                columns: new[] { "ErrorType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_IsResolved",
                table: "AnalyticsErrorLogs",
                column: "IsResolved");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_ResolvedBy",
                table: "AnalyticsErrorLogs",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_ResolvedByUserId",
                table: "AnalyticsErrorLogs",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_Severity",
                table: "AnalyticsErrorLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_Severity_Resolved",
                table: "AnalyticsErrorLogs",
                columns: new[] { "Severity", "IsResolved" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_UserId",
                table: "AnalyticsErrorLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsErrorLogs_UserId1",
                table: "AnalyticsErrorLogs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_CreatedAt",
                table: "AnalyticsPerformanceLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_IsSuccess",
                table: "AnalyticsPerformanceLogs",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_MetricType",
                table: "AnalyticsPerformanceLogs",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_MetricType_CreatedAt",
                table: "AnalyticsPerformanceLogs",
                columns: new[] { "MetricType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_UserId",
                table: "AnalyticsPerformanceLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_UserId_CreatedAt",
                table: "AnalyticsPerformanceLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsPerformanceLogs_UserId1",
                table: "AnalyticsPerformanceLogs",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_CreatedAt",
                table: "AnalyticsUsageLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_FeatureName",
                table: "AnalyticsUsageLogs",
                column: "FeatureName");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_SessionId",
                table: "AnalyticsUsageLogs",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_UsageType",
                table: "AnalyticsUsageLogs",
                column: "UsageType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_UsageType_CreatedAt",
                table: "AnalyticsUsageLogs",
                columns: new[] { "UsageType", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_UserId",
                table: "AnalyticsUsageLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_UserId_CreatedAt",
                table: "AnalyticsUsageLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsUsageLogs_UserId1",
                table: "AnalyticsUsageLogs",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsErrorLogs");

            migrationBuilder.DropTable(
                name: "AnalyticsPerformanceLogs");

            migrationBuilder.DropTable(
                name: "AnalyticsUsageLogs");

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4155));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4157));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4159));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4161));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4162));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4164));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4166));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4167));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4169));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(4170));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(3739));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(3741));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 15, 22, 26, 140, DateTimeKind.Utc).AddTicks(3743));
        }
    }
}
