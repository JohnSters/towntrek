using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Format = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsSuspicious = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsAuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnalyticsAuditLogs_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteBusinesses_BusinessId_CreatedAt",
                table: "FavoriteBusinesses",
                columns: new[] { "BusinessId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReviews_BusinessId_CreatedAt",
                table: "BusinessReviews",
                columns: new[] { "BusinessId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReviews_BusinessId_IsActive_CreatedAt",
                table: "BusinessReviews",
                columns: new[] { "BusinessId", "IsActive", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_Action",
                table: "AnalyticsAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_IpAddress",
                table: "AnalyticsAuditLogs",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_IsSuspicious",
                table: "AnalyticsAuditLogs",
                column: "IsSuspicious");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_Suspicious_Timestamp",
                table: "AnalyticsAuditLogs",
                columns: new[] { "IsSuspicious", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_Timestamp",
                table: "AnalyticsAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_UserId",
                table: "AnalyticsAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_UserId_Timestamp",
                table: "AnalyticsAuditLogs",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsAuditLogs_UserId1",
                table: "AnalyticsAuditLogs",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsAuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteBusinesses_BusinessId_CreatedAt",
                table: "FavoriteBusinesses");

            migrationBuilder.DropIndex(
                name: "IX_BusinessReviews_BusinessId_CreatedAt",
                table: "BusinessReviews");

            migrationBuilder.DropIndex(
                name: "IX_BusinessReviews_BusinessId_IsActive_CreatedAt",
                table: "BusinessReviews");

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5791));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5793));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5795));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5797));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5798));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5800));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5802));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5803));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5805));

            migrationBuilder.UpdateData(
                table: "AdminMessageTopics",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5806));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5352));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5354));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 17, 12, 52, 10, 589, DateTimeKind.Utc).AddTicks(5356));
        }
    }
}
