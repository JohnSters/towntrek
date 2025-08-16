using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TownTrek.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminMessageTopics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IconClass = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ColorClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminMessageTopics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TopicId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AdminResponse = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResponseAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponseBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminMessages_AdminMessageTopics_TopicId",
                        column: x => x.TopicId,
                        principalTable: "AdminMessageTopics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdminMessages_AspNetUsers_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdminMessages_AspNetUsers_ResponseBy",
                        column: x => x.ResponseBy,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AdminMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AdminMessageTopics",
                columns: new[] { "Id", "ColorClass", "CreatedAt", "Description", "IconClass", "IsActive", "Key", "Name", "Priority", "SortOrder" },
                values: new object[,]
                {
                    { 1, "danger", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6882), "Billing problems, payment failures, subscription issues", "fas fa-credit-card", true, "payment-issues", "Payment Issues", "High", 1 },
                    { 2, "danger", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6884), "Site bugs, login issues, functionality not working", "fas fa-bug", true, "technical-problems", "Technical Problems", "High", 2 },
                    { 3, "warning", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6885), "Password resets, account lockouts, permission issues", "fas fa-user-lock", true, "account-access", "Account Access", "High", 3 },
                    { 4, "info", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6887), "New functionality suggestions, improvements", "fas fa-lightbulb", true, "feature-requests", "Feature Requests", "Medium", 4 },
                    { 5, "info", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6889), "Problems with business information, images, approval delays", "fas fa-store", true, "business-listing", "Business Listing Issues", "Medium", 5 },
                    { 6, "info", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6891), "Upgrade/downgrade requests, plan modifications", "fas fa-exchange-alt", true, "subscription-changes", "Subscription Changes", "Medium", 6 },
                    { 7, "warning", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6892), "Incorrect town information, business details", "fas fa-edit", true, "data-corrections", "Data Corrections", "Medium", 7 },
                    { 8, "secondary", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6894), "How-to questions, usage guidance", "fas fa-question-circle", true, "general-support", "General Support", "Low", 8 },
                    { 9, "secondary", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6896), "General feedback about the platform", "fas fa-comment", true, "feedback", "Feedback & Suggestions", "Low", 9 },
                    { 10, "secondary", new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6897), "Business partnerships, collaboration requests", "fas fa-handshake", true, "partnership-inquiries", "Partnership Inquiries", "Low", 10 }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6347));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6349));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 16, 17, 30, 47, 541, DateTimeKind.Utc).AddTicks(6351));

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_CreatedAt",
                table: "AdminMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_Priority",
                table: "AdminMessages",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_ResolvedBy",
                table: "AdminMessages",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_ResponseBy",
                table: "AdminMessages",
                column: "ResponseBy");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_Status",
                table: "AdminMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_Status_Priority",
                table: "AdminMessages",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_TopicId",
                table: "AdminMessages",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessages_UserId",
                table: "AdminMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessageTopics_Key",
                table: "AdminMessageTopics",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminMessageTopics_SortOrder",
                table: "AdminMessageTopics",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminMessages");

            migrationBuilder.DropTable(
                name: "AdminMessageTopics");

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 39, 24, 251, DateTimeKind.Utc).AddTicks(4647));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 39, 24, 251, DateTimeKind.Utc).AddTicks(4649));

            migrationBuilder.UpdateData(
                table: "SubscriptionTiers",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 8, 15, 17, 39, 24, 251, DateTimeKind.Utc).AddTicks(4651));
        }
    }
}
