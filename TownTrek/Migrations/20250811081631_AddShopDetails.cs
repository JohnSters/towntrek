using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddShopDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShopSize = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrandNames = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PriceRange = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Specialties = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HasOnlineStore = table.Column<bool>(type: "bit", nullable: false),
                    OffersLayaway = table.Column<bool>(type: "bit", nullable: false),
                    HasFittingRoom = table.Column<bool>(type: "bit", nullable: false),
                    OffersRepairs = table.Column<bool>(type: "bit", nullable: false),
                    HasLoyaltyProgram = table.Column<bool>(type: "bit", nullable: false),
                    AcceptsReturns = table.Column<bool>(type: "bit", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopDetails_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "FormType",
                value: 6);

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

            migrationBuilder.CreateIndex(
                name: "IX_ShopDetails_BusinessId",
                table: "ShopDetails",
                column: "BusinessId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopDetails");

            migrationBuilder.UpdateData(
                table: "BusinessCategories",
                keyColumn: "Id",
                keyValue: 1,
                column: "FormType",
                value: 0);

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 9, 8, 28, 26, 670, DateTimeKind.Utc).AddTicks(5733));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 9, 8, 28, 26, 670, DateTimeKind.Utc).AddTicks(5735));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 9, 8, 28, 26, 670, DateTimeKind.Utc).AddTicks(5737));
        }
    }
}
