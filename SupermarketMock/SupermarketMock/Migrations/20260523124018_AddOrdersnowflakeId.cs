using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersnowflakeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SnowflakeId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 23, 12, 40, 17, 853, DateTimeKind.Utc).AddTicks(6158), "$2a$11$3ekwNtrm8aFDZJoqfGnsM.vAXsL8yVZHphNqyewBDc0p73NByUwsK" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SnowflakeId",
                table: "Orders",
                column: "SnowflakeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SnowflakeId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SnowflakeId",
                table: "Orders");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2026, 5, 22, 7, 37, 52, 62, DateTimeKind.Utc).AddTicks(4770), "$2a$11$TMCuk1Rp83fMeuAhLT7wMu4DcB670fZYYGsrw0ymluCvuDNZYpjke" });
        }
    }
}
