using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class addRefreshTokenAndRefreshTokenExpiryTimeInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "RefreshToken", "RefreshTokenExpiryTime" },
                values: new object[] { new DateTime(2026, 5, 22, 7, 37, 52, 62, DateTimeKind.Utc).AddTicks(4770), "$2a$11$TMCuk1Rp83fMeuAhLT7wMu4DcB670fZYYGsrw0ymluCvuDNZYpjke", null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiryTime",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 19, 10, 10, 22, 699, DateTimeKind.Utc).AddTicks(9278), "$2a$11$7WI6k20VAcV.eQUjqPJOlOAJZ5gYqkydqsYkPCGKIOyHdiA/l3jxq" });
        }
    }
}
