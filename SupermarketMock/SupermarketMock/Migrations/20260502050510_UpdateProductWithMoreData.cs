using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupermarketMock.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductWithMoreData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Whole milk 1L", "Fresh Milk", 22.90m, 120 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Fresh baked bread", "Whole Wheat Bread", 18.50m, 80 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Farm fresh eggs", "Large Eggs (10pcs)", 28.00m, 65 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "Category", "Description", "DiscountPercentage", "ExpiryDate", "IsAvailable", "IsOnSale", "Name", "OriginalPrice", "Photo", "Price", "Rating", "ReviewCount", "StockQuantity", "SubCategory", "Unit", "Weight" },
                values: new object[,]
                {
                    { 4, null, 1, "Sweet Japan Fuji apple", null, null, true, false, "Red Apple", null, "apple.jpg", 15.90m, null, null, 200, null, "piece", null },
                    { 5, null, 1, "Philippine banana", null, null, true, false, "Banana", null, "banana.jpg", 12.90m, null, null, 150, null, "piece", null },
                    { 6, null, 2, "500g boneless", null, null, true, false, "Chicken Breast", null, "chicken.jpg", 48.00m, null, null, 45, null, "piece", null },
                    { 7, null, 6, "Classic coke", null, null, true, false, "Coca Cola 1.25L", null, "coke.jpg", 9.90m, null, null, 90, null, "piece", null },
                    { 8, null, 7, "Original flavor", null, null, true, false, "Potato Chips", null, "chips.jpg", 14.50m, null, null, 110, null, "piece", null },
                    { 9, null, 11, "Thailand long grain", null, null, true, false, "White Rice 5kg", null, "rice.jpg", 45.00m, null, null, 60, null, "piece", null },
                    { 10, null, 0, "Fresh red tomato", null, null, true, false, "Tomato", null, "tomato.jpg", 8.90m, null, null, 180, null, "piece", null },
                    { 11, null, 4, "150g cup", null, null, true, false, "Strawberry Yogurt", null, "yogurt.jpg", 7.90m, null, null, 95, null, "piece", null },
                    { 12, null, 3, "200g fresh salmon", null, null, true, false, "Salmon Fillet", null, "salmon.jpg", 89.00m, null, null, 30, null, "piece", null },
                    { 13, null, 6, "1L 100% juice", null, null, true, false, "Orange Juice", null, "orangejuice.jpg", 19.90m, null, null, 70, null, "piece", null },
                    { 14, null, 8, "500ml tub", null, null, true, false, "Ice Cream Vanilla", null, "icecream.jpg", 35.00m, null, null, 55, null, "piece", null },
                    { 15, null, 10, "Colgate 100g", null, null, true, false, "Toothpaste", null, "toothpaste.jpg", 16.90m, null, null, 140, null, "piece", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Fresh whole milk", "Milk", 2.49m, 100 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Whole wheat bread", "Bread", 3.09m, 75 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name", "Price", "StockQuantity" },
                values: new object[] { "Dozen large eggs", "Eggs", 4.50m, 50 });
        }
    }
}
