using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialSecurityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTrialUser",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTrialCheck",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrialCheckCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEndDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TrialEndTicks",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "TrialExpired",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TrialSecurityHash",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialStartDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TrialStartTicks",
                table: "AspNetUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TrialAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrialStartTicks = table.Column<long>(type: "bigint", nullable: true),
                    TrialEndTicks = table.Column<long>(type: "bigint", nullable: true),
                    DaysRemaining = table.Column<int>(type: "int", nullable: true),
                    CheckCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrialAuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 13, 6, 18, 7, 427, DateTimeKind.Utc).AddTicks(2597));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 13, 6, 18, 7, 427, DateTimeKind.Utc).AddTicks(2599));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 13, 6, 18, 7, 427, DateTimeKind.Utc).AddTicks(2600));

            migrationBuilder.CreateIndex(
                name: "IX_TrialAuditLogs_Action",
                table: "TrialAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_TrialAuditLogs_Timestamp",
                table: "TrialAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TrialAuditLogs_UserId",
                table: "TrialAuditLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrialAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsTrialUser",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastTrialCheck",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialCheckCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialEndDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialEndTicks",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialExpired",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialSecurityHash",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialStartDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrialStartTicks",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 20, 18, 22, 60, DateTimeKind.Utc).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 20, 18, 22, 60, DateTimeKind.Utc).AddTicks(2032));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 20, 18, 22, 60, DateTimeKind.Utc).AddTicks(2035));
        }
    }
}
