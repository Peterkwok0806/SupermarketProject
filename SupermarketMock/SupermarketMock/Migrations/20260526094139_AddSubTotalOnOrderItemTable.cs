using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AddSubTotalOnOrderItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 26, 9, 41, 39, 408, DateTimeKind.Utc).AddTicks(411), "$2a$11$6jZhESDnyEvP3nU/4HLDi.epkOpuT0YDJx4elZc72LmXlqdA8fc.O" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "OrderItems");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 26, 5, 21, 42, 967, DateTimeKind.Utc).AddTicks(792), "$2a$11$3J/rucxK6yflWshe28BETuqhMP2cOp3Ql7hyk94ix6eHinOTVXX4u" });
        }
    }
}
