using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsVerifiedPurchase = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false),
                    AdminReply = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AdminReplyAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminReplyUserId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewHelpfuls",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewHelpfuls", x => new { x.UserId, x.ReviewId });
                    table.ForeignKey(
                        name: "FK_ReviewHelpfuls_ProductReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ProductReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewHelpfuls_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewImages_ProductReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "ProductReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 6, 26, 8, 40, 47, 98, DateTimeKind.Utc).AddTicks(8701), "$2a$11$mBvUZgsCSEi.A45ITjt4gOlzIB07kkK70G8Q0Z6fwMLRMCh5z6Ssm" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_OrderId",
                table: "ProductReviews",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_ProductId_Status_CreatedAt",
                table: "ProductReviews",
                columns: new[] { "ProductId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId_CreatedAt",
                table: "ProductReviews",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductReviews_UserId_ProductId_OrderId",
                table: "ProductReviews",
                columns: new[] { "UserId", "ProductId", "OrderId" },
                unique: true,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHelpfuls_ReviewId",
                table: "ReviewHelpfuls",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewImages_ReviewId",
                table: "ReviewImages",
                column: "ReviewId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewHelpfuls");

            migrationBuilder.DropTable(
                name: "ReviewImages");

            migrationBuilder.DropTable(
                name: "ProductReviews");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 6, 26, 1, 52, 40, 322, DateTimeKind.Utc).AddTicks(4476), "$2a$11$VfTDDxTG9Yhx.O2QlB.99ODyI6/GIM839N3I8cz8b1o2MvVDQyJHa" });
        }
    }
}
