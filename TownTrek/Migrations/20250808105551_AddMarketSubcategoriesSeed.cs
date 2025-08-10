using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketSubcategoriesSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BusinessSubCategories",
                columns: new[] { "Id", "CategoryId", "IsActive", "Key", "Name" },
                values: new object[,]
                {
                    { 19, 3, true, "farmers", "Farmers Market" },
                    { 20, 3, true, "craft", "Craft Market" },
                    { 21, 3, true, "flea", "Flea Market" },
                    { 22, 3, true, "food", "Food Market" },
                    { 23, 3, true, "antique", "Antique Market" }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 55, 51, 97, DateTimeKind.Utc).AddTicks(1782));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 55, 51, 97, DateTimeKind.Utc).AddTicks(1784));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 55, 51, 97, DateTimeKind.Utc).AddTicks(1786));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "BusinessSubCategories",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 35, 30, 70, DateTimeKind.Utc).AddTicks(2888));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 35, 30, 70, DateTimeKind.Utc).AddTicks(2890));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 8, 10, 35, 30, 70, DateTimeKind.Utc).AddTicks(2892));
        }
    }
}
