using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AllPromotionAndProductPromotionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsOnSale",
                table: "Products");

            migrationBuilder.AddColumn<long>(
                name: "SnowflakeId",
                table: "Products",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Promotions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BuyQuantity = table.Column<int>(type: "int", nullable: true),
                    FreeQuantity = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductPromotions",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PromotionId = table.Column<int>(type: "int", nullable: false),
                    OverrideStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OverrideEndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPromotions", x => new { x.ProductId, x.PromotionId });
                    table.ForeignKey(
                        name: "FK_ProductPromotions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPromotions_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                column: "SnowflakeId",
                value: null);

            migrationBuilder.InsertData(
                table: "Promotions",
                columns: new[] { "Id", "BuyQuantity", "DiscountValue", "EndDate", "FreeQuantity", "Name", "StartDate", "Type" },
                values: new object[,]
                {
                    { 1, null, 20m, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Summer Dairy 20% OFF", new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "PercentageOff" },
                    { 2, null, 15m, new DateTime(2026, 5, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Meat & Seafood Flash Sale", new DateTime(2026, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "FixedDiscount" },
                    { 3, 2, null, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "Soft Drinks Buy 2 Get 1 FREE", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "BuyXGetYFree" },
                    { 4, 2, 25m, new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Snacks Bundle Offer", new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "QuantitySpecialPrice" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 26, 3, 12, 59, 325, DateTimeKind.Utc).AddTicks(8220), "$2a$11$Wg09rqBbC0.0cxWPKGMl9eSf38nVpGckye6G.iAcTrJ7T.Qsk.y6." });

            migrationBuilder.InsertData(
                table: "ProductPromotions",
                columns: new[] { "ProductId", "PromotionId", "OverrideEndDate", "OverrideStartDate" },
                values: new object[,]
                {
                    { 1, 1, null, null },
                    { 6, 2, null, null },
                    { 7, 3, null, null },
                    { 8, 4, null, null },
                    { 11, 1, new DateTime(2026, 7, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 12, 2, null, null },
                    { 13, 3, new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPromotions_PromotionId",
                table: "ProductPromotions",
                column: "PromotionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPromotions");

            migrationBuilder.DropTable(
                name: "Promotions");

            migrationBuilder.DropColumn(
                name: "SnowflakeId",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnSale",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "DiscountPercentage", "IsOnSale" },
                values: new object[] { null, false });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 23, 12, 40, 17, 853, DateTimeKind.Utc).AddTicks(6158), "$2a$11$3ekwNtrm8aFDZJoqfGnsM.vAXsL8yVZHphNqyewBDc0p73NByUwsK" });
        }
    }
}
