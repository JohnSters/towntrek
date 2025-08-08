using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupTablesForBusiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FormType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BusinessSubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessSubCategories_BusinessCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "BusinessCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BusinessCategories",
                columns: new[] { "Id", "Description", "FormType", "IconClass", "IsActive", "Key", "Name" },
                values: new object[,]
                {
                    { 1, "Local shops and retail businesses", 0, "fas fa-shopping-bag", true, "shops-retail", "Shops & Retail" },
                    { 2, "Restaurants, cafes, and food services", 1, "fas fa-utensils", true, "restaurants-food", "Restaurants & Food Services" },
                    { 3, "Local markets and vendor stalls", 2, "fas fa-store", true, "markets-vendors", "Markets & Vendors" },
                    { 4, "Hotels, guesthouses, and lodging", 5, "fas fa-bed", true, "accommodation", "Accommodation" },
                    { 5, "Tour guides and experience providers", 3, "fas fa-map-marked-alt", true, "tours-experiences", "Tours & Experiences" },
                    { 6, "Local events and entertainment", 4, "fas fa-calendar-alt", true, "events", "Events" }
                });

            migrationBuilder.InsertData(
                table: "ServiceDefinitions",
                columns: new[] { "Id", "IsActive", "Key", "Name" },
                values: new object[,]
                {
                    { 1, true, "delivery", "Delivery Available" },
                    { 2, true, "takeaway", "Takeaway/Collection" },
                    { 3, true, "wheelchair", "Wheelchair Accessible" },
                    { 4, true, "parking", "Parking Available" },
                    { 5, true, "wifi", "Free WiFi" },
                    { 6, true, "cards", "Card Payments Accepted" }
                });

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

            migrationBuilder.InsertData(
                table: "BusinessSubCategories",
                columns: new[] { "Id", "CategoryId", "IsActive", "Key", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, "clothing", "Clothing & Fashion" },
                    { 2, 1, true, "electronics", "Electronics" },
                    { 3, 1, true, "books", "Books & Stationery" },
                    { 4, 1, true, "gifts", "Gifts & Souvenirs" },
                    { 5, 1, true, "hardware", "Hardware & Tools" },
                    { 6, 1, true, "pharmacy", "Pharmacy & Health" },
                    { 7, 2, true, "restaurant", "Restaurant" },
                    { 8, 2, true, "cafe", "Cafe & Coffee Shop" },
                    { 9, 2, true, "fast-food", "Fast Food" },
                    { 10, 2, true, "bakery", "Bakery" },
                    { 11, 2, true, "bar", "Bar & Pub" },
                    { 12, 2, true, "takeaway", "Takeaway" },
                    { 13, 4, true, "hotel", "Hotel" },
                    { 14, 4, true, "guesthouse", "Guesthouse" },
                    { 15, 4, true, "bnb", "Bed & Breakfast" },
                    { 16, 4, true, "self-catering", "Self-catering" },
                    { 17, 4, true, "backpackers", "Backpackers" },
                    { 18, 4, true, "camping", "Camping & Caravan" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCategories_Key",
                table: "BusinessCategories",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessSubCategories_CategoryId_Key",
                table: "BusinessSubCategories",
                columns: new[] { "CategoryId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDefinitions_Key",
                table: "ServiceDefinitions",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessSubCategories");

            migrationBuilder.DropTable(
                name: "ServiceDefinitions");

            migrationBuilder.DropTable(
                name: "BusinessCategories");

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
    }
}
