using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using SupermarketMock.Models;
using SupermarketMock.Services;
using SupermarketMock.DTOs;
using IdGen;

namespace SupermarketMock.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IIdGenerator<long>> _idGeneratorMock;
        private readonly SupermarketContext _context;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _idGeneratorMock = new Mock<IIdGenerator<long>>();
            _idGeneratorMock.Setup(x => x.CreateId()).Returns(1234567890123456789L);

            var options = new DbContextOptionsBuilder<SupermarketContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                })
                .Options;

            _context = new SupermarketContext(options);
            _service = new OrderService(_context, _idGeneratorMock.Object);
        }

        [Fact]
        public async Task CreateOrderAsync_BuyXGetYFree_Success()
        {
            // Arrange
            var product = SeedProduct(1, stock: 20, price: 100m);
            SeedPromotion(product, PromotionType.BuyXGetYFree, buyQty: 2, freeQty: 1, priority: 10);
            SeedCartItem(1, productId: 1, quantity: 5);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            // Assert
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(400m, result.Order?.totalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_QuantitySpecialPrice_Success()
        {
            var product = SeedProduct(1, stock: 20, price: 100m);
            SeedPromotion(product, PromotionType.QuantitySpecialPrice, buyQty: 3, discountValue: 250m, priority: 10);
            SeedCartItem(1, productId: 1, quantity: 7);

            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(600m, result.Order?.totalAmount);
        }

        [Fact]
        public async Task CreateOrderAsync_MultipleProducts_LocksInOrder_Success()
        {
            SeedProduct(1, stock: 10, price: 200m);
            SeedProduct(5, stock: 10, price: 100m);
            SeedProduct(10, stock: 10, price: 150m);

            SeedCartItem(1, productId: 5, quantity: 1);
            SeedCartItem(1, productId: 1, quantity: 2);
            SeedCartItem(1, productId: 10, quantity: 1);

            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(3, result.Order?.orderItems?.Count ?? 0);
        }

        private CreateOrderDto CreateValidDto() => new CreateOrderDto
        {
            FullName = "測試用戶",
            Phone = "0912345678",
            Address = "測試地址"
        };

        private Product SeedProduct(int id, int stock, decimal price, string name = null)
        {
            var product = new Product
            {
                Id = id,
                Name = name ?? $"商品{id}",
                StockQuantity = stock,
                Price = price
            };

            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }

        private void SeedCartItem(int userId, int productId, int quantity)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId)
                       ?? new Cart { UserId = userId };

            if (cart.Id == 0)
            {
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity
            };

            _context.CartItems.Add(cartItem);
            _context.SaveChanges();
        }

        private void SeedPromotion(Product product, PromotionType type,
            int? buyQty = null, int? freeQty = null, decimal? discountValue = null, int priority = 0)
        {
            var promotion = new Promotion
            {
                Id = 100 + product.Id,
                Type = type,
                BuyQuantity = buyQty,
                FreeQuantity = freeQty,
                DiscountValue = discountValue,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            var productPromotion = new ProductPromotion
            {
                ProductId = product.Id,
                PromotionId = promotion.Id,
                Priority = priority,
                Promotion = promotion,
                // 如果有 Override 日期，也可加上
            };

            _context.Promotions.Add(promotion);
            _context.ProductPromotions.Add(productPromotion);
            _context.SaveChanges();
        }
    }
}