using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsActive", "PasswordHash" },
                values: new object[] { new DateTime(2026, 6, 25, 14, 9, 14, 258, DateTimeKind.Utc).AddTicks(5445), true, "$2a$11$Fbduhczjy5C5XyADZuYx4OgQ5wQKAuZ.6sD1F4Sm5UBcTc9AlFO7S" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 31, 4, 48, 2, 520, DateTimeKind.Utc).AddTicks(6997), "$2a$11$XR2l3v8S4PuROpVjZWc33upDNLeF29ceoE12JAWXM7ZVzfrbOggia" });
        }
    }
}
