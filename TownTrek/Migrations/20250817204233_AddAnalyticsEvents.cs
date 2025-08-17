using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: true),
                    Platform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnalyticsEvents_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id");
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_BusinessId",
                table: "AnalyticsEvents",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId",
                table: "AnalyticsEvents",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

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
        }
    }
}
