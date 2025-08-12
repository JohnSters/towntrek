using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAntiqueShopSubcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BusinessSubCategories",
                columns: new[] { "Id", "CategoryId", "IsActive", "Key", "Name" },
                values: new object[] { 24, 1, true, "antique-shop", "Antique Shop" });

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 47, 42, 331, DateTimeKind.Utc).AddTicks(5146));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 47, 42, 331, DateTimeKind.Utc).AddTicks(5148));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 47, 42, 331, DateTimeKind.Utc).AddTicks(5150));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 16, 30, 225, DateTimeKind.Utc).AddTicks(177));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 16, 30, 225, DateTimeKind.Utc).AddTicks(186));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 11, 8, 16, 30, 225, DateTimeKind.Utc).AddTicks(188));
        }
    }
}
