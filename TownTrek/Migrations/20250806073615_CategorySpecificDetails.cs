using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class CategorySpecificDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccommodationDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StarRating = table.Column<int>(type: "int", nullable: true),
                    RoomCount = table.Column<int>(type: "int", nullable: true),
                    MaxGuests = table.Column<int>(type: "int", nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PricingInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasWiFi = table.Column<bool>(type: "bit", nullable: false),
                    HasParking = table.Column<bool>(type: "bit", nullable: false),
                    HasPool = table.Column<bool>(type: "bit", nullable: false),
                    HasRestaurant = table.Column<bool>(type: "bit", nullable: false),
                    HasGym = table.Column<bool>(type: "bit", nullable: false),
                    HasSpa = table.Column<bool>(type: "bit", nullable: false),
                    IsPetFriendly = table.Column<bool>(type: "bit", nullable: false),
                    HasAirConditioning = table.Column<bool>(type: "bit", nullable: false),
                    RoomTypes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresDeposit = table.Column<bool>(type: "bit", nullable: false),
                    CancellationPolicy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationDetails_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    AlertType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPushNotification = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessAlerts_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecurrenceEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VenueAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxAttendees = table.Column<int>(type: "int", nullable: true),
                    ExpectedAttendance = table.Column<int>(type: "int", nullable: true),
                    TicketInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrganizerContact = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresTickets = table.Column<bool>(type: "bit", nullable: false),
                    IsFreeEvent = table.Column<bool>(type: "bit", nullable: false),
                    EventProgram = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AgeRestrictions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasParking = table.Column<bool>(type: "bit", nullable: false),
                    HasRefreshments = table.Column<bool>(type: "bit", nullable: false),
                    IsOutdoorEvent = table.Column<bool>(type: "bit", nullable: false),
                    HasWeatherBackup = table.Column<bool>(type: "bit", nullable: false),
                    EventStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventDetails_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarketDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarketStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    MarketEndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EstimatedVendorCount = table.Column<int>(type: "int", nullable: true),
                    VendorTypes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParkingInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecialEvents = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EntryFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HasRestrooms = table.Column<bool>(type: "bit", nullable: false),
                    HasFoodVendors = table.Column<bool>(type: "bit", nullable: false),
                    IsCoveredVenue = table.Column<bool>(type: "bit", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketDetails_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CuisineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PriceRange = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    HasDelivery = table.Column<bool>(type: "bit", nullable: false),
                    HasTakeaway = table.Column<bool>(type: "bit", nullable: false),
                    AcceptsReservations = table.Column<bool>(type: "bit", nullable: false),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: true),
                    SeatingCapacity = table.Column<int>(type: "int", nullable: true),
                    DietaryOptions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MenuUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    HasKidsMenu = table.Column<bool>(type: "bit", nullable: false),
                    HasOutdoorSeating = table.Column<bool>(type: "bit", nullable: false),
                    HasPrivateDining = table.Column<bool>(type: "bit", nullable: false),
                    HasLiveMusic = table.Column<bool>(type: "bit", nullable: false),
                    SpecialFeatures = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServesBreakfast = table.Column<bool>(type: "bit", nullable: false),
                    ServesLunch = table.Column<bool>(type: "bit", nullable: false),
                    ServesDinner = table.Column<bool>(type: "bit", nullable: false),
                    ServesAlcohol = table.Column<bool>(type: "bit", nullable: false),
                    BreakfastStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    BreakfastEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    LunchStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    LunchEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    DinnerStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    DinnerEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantDetails_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpecialOperatingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialOperatingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecialOperatingHours_Businesses_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: true),
                    MinGroupSize = table.Column<int>(type: "int", nullable: true),
                    MinAge = table.Column<int>(type: "int", nullable: true),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    DifficultyLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartureLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Itinerary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IncludedItems = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExcludedItems = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequiredEquipment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PricingInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RequiresBooking = table.Column<bool>(type: "bit", nullable: false),
                    AdvanceBookingDays = table.Column<int>(type: "int", nullable: true),
                    AvailableDays = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableSeasons = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsWeatherDependent = table.Column<bool>(type: "bit", nullable: false),
                    HasInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsAccessible = table.Column<bool>(type: "bit", nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourDetails_Businesses_BusinessId",
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
                value: new DateTime(2025, 8, 6, 7, 36, 14, 678, DateTimeKind.Utc).AddTicks(3347));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 7, 36, 14, 678, DateTimeKind.Utc).AddTicks(3349));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 6, 7, 36, 14, 678, DateTimeKind.Utc).AddTicks(3351));

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationDetails_BusinessId",
                table: "AccommodationDetails",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAlerts_BusinessId",
                table: "BusinessAlerts",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_EventDetails_BusinessId",
                table: "EventDetails",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketDetails_BusinessId",
                table: "MarketDetails",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantDetails_BusinessId",
                table: "RestaurantDetails",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialOperatingHours_BusinessId",
                table: "SpecialOperatingHours",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDetails_BusinessId",
                table: "TourDetails",
                column: "BusinessId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationDetails");

            migrationBuilder.DropTable(
                name: "BusinessAlerts");

            migrationBuilder.DropTable(
                name: "EventDetails");

            migrationBuilder.DropTable(
                name: "MarketDetails");

            migrationBuilder.DropTable(
                name: "RestaurantDetails");

            migrationBuilder.DropTable(
                name: "SpecialOperatingHours");

            migrationBuilder.DropTable(
                name: "TourDetails");

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
        }
    }
}
