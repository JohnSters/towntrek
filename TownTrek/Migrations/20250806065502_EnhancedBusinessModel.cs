using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class EnhancedBusinessModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_AspNetUsers_ApplicationUserId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "AdditionalNotes",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ImagesPath",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "SpecialOffers",
                table: "Businesses",
                newName: "ShortDescription");

            migrationBuilder.RenameColumn(
                name: "Services",
                table: "Businesses",
                newName: "LogoUrl");

            migrationBuilder.RenameColumn(
                name: "OperatingHours",
                table: "Businesses",
                newName: "CoverImageUrl");

            migrationBuilder.RenameColumn(
                name: "LogoPath",
                table: "Businesses",
                newName: "ApprovedBy");

            migrationBuilder.RenameColumn(
                name: "IsApproved",
                table: "Businesses",
                newName: "IsVerified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Businesses",
                newName: "IsFeatured");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Businesses",
                newName: "ApprovedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_ApplicationUserId",
                table: "Businesses",
                newName: "IX_Businesses_ApprovedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Businesses",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Businesses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Businesses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Businesses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber2",
                table: "Businesses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Businesses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Businesses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "SubCategory",
                table: "Businesses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalReviews",
                table: "Businesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Businesses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BusinessContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    ContactType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessContacts_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsOpen = table.Column<bool>(type: "bit", nullable: false),
                    IsSpecialHours = table.Column<bool>(type: "bit", nullable: false),
                    SpecialHoursNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessHours_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    ImageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessImages_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BusinessImages_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessServices_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 6, 55, 1, 912, DateTimeKind.Utc).AddTicks(5481));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 6, 55, 1, 912, DateTimeKind.Utc).AddTicks(5483));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 6, 55, 1, 912, DateTimeKind.Utc).AddTicks(5485));

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_UserId",
                table: "Businesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessContacts_BusinessId",
                table: "BusinessContacts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessHours_BusinessId",
                table: "BusinessHours",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessImages_ApprovedByUserId",
                table: "BusinessImages",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessImages_BusinessId",
                table: "BusinessImages",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessServices_BusinessId",
                table: "BusinessServices",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_AspNetUsers_ApprovedByUserId",
                table: "Businesses",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_AspNetUsers_UserId",
                table: "Businesses",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_AspNetUsers_ApprovedByUserId",
                table: "Businesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Businesses_AspNetUsers_UserId",
                table: "Businesses");

            migrationBuilder.DropTable(
                name: "BusinessContacts");

            migrationBuilder.DropTable(
                name: "BusinessHours");

            migrationBuilder.DropTable(
                name: "BusinessImages");

            migrationBuilder.DropTable(
                name: "BusinessServices");

            migrationBuilder.DropIndex(
                name: "IX_Businesses_UserId",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "PhoneNumber2",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "SubCategory",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "TotalReviews",
                table: "Businesses");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Businesses");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "Businesses",
                newName: "SpecialOffers");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "Businesses",
                newName: "Services");

            migrationBuilder.RenameColumn(
                name: "IsVerified",
                table: "Businesses",
                newName: "IsApproved");

            migrationBuilder.RenameColumn(
                name: "IsFeatured",
                table: "Businesses",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "CoverImageUrl",
                table: "Businesses",
                newName: "OperatingHours");

            migrationBuilder.RenameColumn(
                name: "ApprovedByUserId",
                table: "Businesses",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "Businesses",
                newName: "LogoPath");

            migrationBuilder.RenameIndex(
                name: "IX_Businesses_ApprovedByUserId",
                table: "Businesses",
                newName: "IX_Businesses_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "AdditionalNotes",
                table: "Businesses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagesPath",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8096));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8098));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 5, 9, 54, 16, 358, DateTimeKind.Utc).AddTicks(8100));

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_AspNetUsers_ApplicationUserId",
                table: "Businesses",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
