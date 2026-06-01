using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;
using SupermarketMock.Services;
using Xunit;

namespace SupermarketMock.Tests
{
    public class ProductServiceTests
    {
        // 輔助方法：每次測試都建立一個全新的、乾淨的記憶體資料庫 Context
        private SupermarketContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                // 使用 Guid 作為資料庫名稱，確保每個測試案例之間的資料完全隔離
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new SupermarketContext(options);
        }

        [Fact]
        public async Task GetProductsAsync_WhenCategoryIsNull_ShouldReturnAllProducts()
        {
            // === 1. Arrange (準備資料) ===
            using var context = GetInMemoryDbContext();

            context.Products.AddRange(new List<Product>
            {
                new Product { Id = 1, Name = "蘋果", Price = 30m, CategoryId = 1, SnowflakeId = 101, Photo = "apple.jpg" },
                new Product { Id = 2, Name = "香蕉", Price = 20m, CategoryId = 1, SnowflakeId = 102, Photo = "banana.jpg" },
                new Product { Id = 3, Name = "鮮乳", Price = 80m, CategoryId = 2, SnowflakeId = 103, Photo = "milk.jpg" }
            });
            await context.SaveChangesAsync();

            var productService = new ProductService(context);

            // === 2. Act (執行方法) ===
            var result = await productService.GetProductsAsync(category: null);

            // === 3. Assert (驗證結果) ===
            var productList = result.ToList();
            Assert.Equal(3, productList.Count); // 應該要拿到全部 3 筆商品
        }

        [Fact]
        public async Task GetProductsAsync_WhenCategoryHasValue_ShouldReturnFilteredProducts()
        {
            // === 1. Arrange (準備資料) ===
            using var context = GetInMemoryDbContext();

            context.Products.AddRange(new List<Product>
            {
                new Product { Id = 1, Name = "蘋果", Price = 30m, CategoryId = 1, SnowflakeId = 101, Photo = "apple.jpg" },
                new Product { Id = 2, Name = "香蕉", Price = 20m, CategoryId = 1, SnowflakeId = 102, Photo = "banana.jpg" },
                new Product { Id = 3, Name = "鮮乳", Price = 80m, CategoryId = 2, SnowflakeId = 103, Photo = "milk.jpg" }
            });
            await context.SaveChangesAsync();

            var productService = new ProductService(context);

            // === 2. Act (執行方法：只查詢分類 ID 為 1 的商品) ===
            var result = await productService.GetProductsAsync(category: 1);

            // === 3. Assert (驗證結果) ===
            var productList = result.ToList();
            Assert.Equal(2, productList.Count); // 應該只篩選出 2 筆商品
        }
    }
}
