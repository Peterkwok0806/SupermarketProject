using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLoginAtToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 4, 4, 18, 55, 175, DateTimeKind.Utc).AddTicks(9981), "$2a$11$/HJMsSjjoqe34QpxxlmhF..joqCqRytissLJQKqbdGE.zHijnp.Me" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 4, 4, 10, 25, 581, DateTimeKind.Utc).AddTicks(5019), "$2a$11$BTqrrKJQVwKL2XYH12lyMuo6op7LYdj4Vur/./35fiaIMeqMHWUBu" });
        }
    }
}
