using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToBusinessImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BusinessImages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 8, 29, 23, 629, DateTimeKind.Utc).AddTicks(6960));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 8, 29, 23, 629, DateTimeKind.Utc).AddTicks(6961));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 7, 8, 29, 23, 629, DateTimeKind.Utc).AddTicks(6963));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BusinessImages");

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 7, 46, 27, 111, DateTimeKind.Utc).AddTicks(412));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 7, 46, 27, 111, DateTimeKind.Utc).AddTicks(414));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 7, 46, 27, 111, DateTimeKind.Utc).AddTicks(416));
        }
    }
}
