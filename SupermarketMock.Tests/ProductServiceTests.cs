using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SupermarketMock.Models;
using SupermarketMock.Services;
using Xunit;

namespace SupermarketMock.Tests
{
    
    public class ProductServiceTests
    {
        private readonly SupermarketContext _context;
        private readonly ProductService _service;


        public ProductServiceTests() 
        {
            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new SupermarketContext(options);
            _service = new ProductService(_context);
        }


        [Fact]
        public async Task GetProductsAsync_WhenCategoryIsNull_ShouldReturnPagedAllProducts()
        {
            //1. Arrange Product Seed Data
            SeedProduct(1, catid: 1, stock: 30, price: 30m, name: "Apple");
            SeedProduct(2, catid: 1, stock: 40, price: 20m, name: "Banana");
            SeedProduct(3, catid: 2, stock: 50, price: 80m, name: "Milk");

            //2. Act 傳回所有的Products
            var result = await _service.GetProductsAsync(category: null, pageNumber: 1, pageSize: 10);

            //3. Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);  // 全站總筆數應為 3
            Assert.Equal(1, result.PageNumber);  // 目前頁碼應為 1
            Assert.Equal(1, result.TotalPages);  // 總頁數應為 1

            var productList = result.Items.ToList();
            Assert.Equal(3, productList.Count);
            Assert.Equal("Apple", productList[0].name);

        }

        [Fact]
        public async Task GetProductsAsync_WhenCategoryHasValue_ShouldReturnPagedAndFilteredProducts()
        {
            //1. Arrange Product Seed Data
            SeedProduct(1, catid: 1, stock: 30, price: 30m, name: "Apple");
            SeedProduct(2, catid: 1, stock: 40, price: 20m, name: "Banana");
            SeedProduct(3, catid: 2, stock: 50, price: 80m, name: "Milk");

            //2. Act 傳回只有catid == 1的Products
            var result = await _service.GetProductsAsync(category: 1, pageNumber: 1, pageSize: 10);

            //3. Assert 
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);

            var productList = result.Items.ToList();
            Assert.Equal(2, productList.Count);

            var expectedIds = new List<int> { 1, 2 };
            Assert.Equal(expectedIds, productList.Select(dto => dto.id).OrderBy(id => id));

        }

        [Fact]
        public async Task GetProductSuggestionsAsync_WhenTriggered_ShouldReturnLimitToMax8DistinctNames()
        {
            // 1. Arrange (準備 10 個名字重複或相似的商品，測試 8 筆限制優化) ===
            for (int i = 1; i <= 10; i++)
            {
                SeedProduct(i, catid: 1, stock: 30, price: 30m, name: $"Coke{i}");
            }

            // 2. Act
            var suggestions = await _service.GetProductSuggestionsAsync(query:"Coke");

            //3. Assert
            var suggestionList = suggestions.ToList();
            Assert.True(suggestionList.Count <= 8, $"Suggestions count should be limited to 8, but got {suggestionList.Count}");
            Assert.All(suggestionList, name => Assert.Contains("coke", name.ToLower()));
        }



        private Product SeedProduct(int id,int catid, int stock, decimal price, string name = null)
        {
            var product = new Product
            {
                Id = id,
                Name = name ?? $"商品{id}",
                CategoryId = catid,
                StockQuantity = stock,
                Price = price
            };

            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }

    }
}
