using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductPromotionSeedWithPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "ProductPromotions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 1, 1 },
                column: "Priority",
                value: 10);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 6, 2 },
                column: "Priority",
                value: 20);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 7, 3 },
                column: "Priority",
                value: 5);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 8, 4 },
                column: "Priority",
                value: 1);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 11, 1 },
                column: "Priority",
                value: 10);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 12, 2 },
                column: "Priority",
                value: 20);

            migrationBuilder.UpdateData(
                table: "ProductPromotions",
                keyColumns: new[] { "ProductId", "PromotionId" },
                keyValues: new object[] { 13, 3 },
                column: "Priority",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 26, 5, 21, 42, 967, DateTimeKind.Utc).AddTicks(792), "$2a$11$3J/rucxK6yflWshe28BETuqhMP2cOp3Ql7hyk94ix6eHinOTVXX4u" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Priority",
                table: "ProductPromotions");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 26, 3, 56, 56, 794, DateTimeKind.Utc).AddTicks(380), "$2a$11$uvvjFrsD..fxYjtT4Ob1MuLheZKEEO0K26Ev4nTiTOXD2HqVAXUwC" });
        }
    }
}
