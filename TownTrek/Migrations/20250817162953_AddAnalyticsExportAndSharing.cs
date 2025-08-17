using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsExportAndSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEmailReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SendCount = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEmailReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsEmailReports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsEmailReports_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalyticsEmailReports_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsEmailReports_Businesses_BusinessId1",
                        column: x => x.BusinessId1,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsShareableLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LinkToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DashboardType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AccessCount = table.Column<int>(type: "int", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsShareableLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsShareableLinks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsShareableLinks_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalyticsShareableLinks_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsShareableLinks_Businesses_BusinessId1",
                        column: x => x.BusinessId1,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_BusinessId",
                table: "AnalyticsEmailReports",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_BusinessId1",
                table: "AnalyticsEmailReports",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_CreatedAt",
                table: "AnalyticsEmailReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_Frequency",
                table: "AnalyticsEmailReports",
                column: "Frequency");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_IsActive",
                table: "AnalyticsEmailReports",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_NextScheduled",
                table: "AnalyticsEmailReports",
                column: "NextScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_ReportType",
                table: "AnalyticsEmailReports",
                column: "ReportType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_UserId",
                table: "AnalyticsEmailReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_UserId_Active",
                table: "AnalyticsEmailReports",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEmailReports_UserId1",
                table: "AnalyticsEmailReports",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_BusinessId",
                table: "AnalyticsShareableLinks",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_BusinessId1",
                table: "AnalyticsShareableLinks",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_CreatedAt",
                table: "AnalyticsShareableLinks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_DashboardType",
                table: "AnalyticsShareableLinks",
                column: "DashboardType");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_ExpiresAt",
                table: "AnalyticsShareableLinks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_IsActive",
                table: "AnalyticsShareableLinks",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_LinkToken",
                table: "AnalyticsShareableLinks",
                column: "LinkToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_Token_Active",
                table: "AnalyticsShareableLinks",
                columns: new[] { "LinkToken", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_UserId",
                table: "AnalyticsShareableLinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsShareableLinks_UserId1",
                table: "AnalyticsShareableLinks",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEmailReports");

            migrationBuilder.DropTable(
                name: "AnalyticsShareableLinks");

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
        }
    }
}
