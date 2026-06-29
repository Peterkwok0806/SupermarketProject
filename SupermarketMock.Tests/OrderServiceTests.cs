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
        public async Task CreateOrderAsync_BuyXGetYFree_SuccessAndDeductStock()
        {
            // Arrange
            var product = SeedProduct(1, stock: 20, price: 100m);
            SeedPromotion(product, PromotionType.BuyXGetYFree, buyQty: 2, freeQty: 1, priority: 10);
            SeedCartItem(1, productId: 1, quantity: 5);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            // Assert 
            //  // Validation A: Pricing integrity
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(400m, result.Order?.totalAmount);

            // Validation B
            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.NotNull(updatedProduct);
            Assert.Equal(15, updatedProduct.StockQuantity);
        }

        [Fact]
        public async Task CreateOrderAsync_QuantitySpecialPrice_SuccessAndDeductStock()
        {
            var product = SeedProduct(1, stock: 20, price: 100m);
            SeedPromotion(product, PromotionType.QuantitySpecialPrice, buyQty: 3, discountValue: 250m, priority: 10);
            SeedCartItem(1, productId: 1, quantity: 7);

            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            // Validation A: Pricing integrity
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(600m, result.Order?.totalAmount);

            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.NotNull(updatedProduct);
            Assert.Equal(13, updatedProduct.StockQuantity);
        }

        [Fact]
        public async Task CreateOrderAsync_WhenStockIsInsufficient_ShouldRollbackAndReturnErrorMessage()
        {
            SeedProduct(1, stock: 5, price: 200m);

            SeedCartItem(1, productId: 1, quantity: 10);
          

            var result = await _service.CreateOrderAsync(1, CreateValidDto());

            Assert.False(result.Success);
            Assert.Contains("庫存不足", result.Message);

            // Validation A: Zero inventory corruption (Stock must remain unchanged at 5)
            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.NotNull(updatedProduct);
            Assert.Equal(5, updatedProduct.StockQuantity);

            // Validation B: Transaction abort check (No corrupt orders added to DB)
            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);
        }

        /// <summary>
        /// 建立預設的訂單 DTO，可選帶入優惠券代碼。
        /// </summary>
        private CreateOrderDto CreateValidDto(string? couponCode = null) => new CreateOrderDto
        {
            FullName = "測試用戶",
            Phone = "0912345678",
            Address = "測試地址",
            CouponCode = couponCode
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

        private Coupon SeedCoupon(string code, CouponType type, decimal discountValue,
            decimal? minimumOrderAmount = null, decimal? maximumDiscountAmount = null,
            int? usageLimit = null, int usedCount = 0, int? usageLimitPerUser = null,
            bool isActive = true, CouponScope scope = CouponScope.Global)
        {
            var coupon = new Coupon
            {
                Code = code.ToUpper(),
                Type = type,
                DiscountValue = discountValue,
                MinimumOrderAmount = minimumOrderAmount,
                MaximumDiscountAmount = maximumDiscountAmount,
                UsageLimit = usageLimit,
                UsedCount = usedCount,
                UsageLimitPerUser = usageLimitPerUser,
                Scope = scope,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(30),
                IsActive = isActive
            };
            _context.Coupons.Add(coupon);
            _context.SaveChanges();
            return coupon;
        }

        private void SeedCouponProduct(int couponId, int productId)
        {
            _context.CouponProducts.Add(new CouponProduct
            {
                CouponId = couponId,
                ProductId = productId
            });
            _context.SaveChanges();
        }

        // ==================== 優惠券：正向測試 ====================

        [Fact]
        public async Task CreateOrderAsync_WithPercentageCoupon_ShouldApplyDiscountAndRecordUsage()
        {
            // Arrange：商品 $100 × 5 = $500，10% 折扣 → $50
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            var coupon = SeedCoupon("SAVE10", CouponType.Percentage, discountValue: 10m);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("SAVE10"));

            // Assert：訂單金額 $450
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(450m, result.Order?.totalAmount);

            // 驗證訂單優惠券欄位
            var savedOrder = await _context.Orders.FirstAsync();
            Assert.Equal(coupon.Id, savedOrder.CouponId);
            Assert.Equal(50m, savedOrder.DiscountAmount);

            // 驗證 CouponUsage 記錄已建立
            var usage = await _context.CouponUsages.FirstOrDefaultAsync(u => u.CouponId == coupon.Id);
            Assert.NotNull(usage);
            Assert.Equal(1, usage!.UserId);
            Assert.Equal(50m, usage.DiscountApplied);

            // 驗證 UsedCount 已累加
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.Id);
            Assert.NotNull(updatedCoupon);
            Assert.Equal(1, updatedCoupon!.UsedCount);
        }

        [Fact]
        public async Task CreateOrderAsync_WithFixedAmountCoupon_ShouldApplyDiscountAndRecordUsage()
        {
            // Arrange：商品 $100 × 5 = $500，固定折 $30
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            var coupon = SeedCoupon("FIX30", CouponType.FixedAmount, discountValue: 30m);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("FIX30"));

            // Assert：訂單金額 $470
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(470m, result.Order?.totalAmount);

            var savedOrder = await _context.Orders.FirstAsync();
            Assert.Equal(coupon.Id, savedOrder.CouponId);
            Assert.Equal(30m, savedOrder.DiscountAmount);

            // 驗證 CouponUsage 記錄
            var usage = await _context.CouponUsages.FirstOrDefaultAsync(u => u.CouponId == coupon.Id);
            Assert.NotNull(usage);
            Assert.Equal(30m, usage!.DiscountApplied);

            // 驗證 UsedCount
            var updatedCoupon = await _context.Coupons.FindAsync(coupon.Id);
            Assert.Equal(1, updatedCoupon!.UsedCount);
        }

        [Fact]
        public async Task CreateOrderAsync_WithPercentageCouponExceedingMaxDiscount_ShouldCapAtMaximum()
        {
            // Arrange：商品 $100 × 5 = $500，50% 折扣 但上限 $25
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            var coupon = SeedCoupon("BIG50", CouponType.Percentage, discountValue: 50m,
                maximumDiscountAmount: 25m);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("BIG50"));

            // Assert：折扣被限制在 $25，訂單金額 $475
            Assert.True(result.Success, $"建立訂單失敗: {result.Message}");
            Assert.Equal(475m, result.Order?.totalAmount);

            var savedOrder = await _context.Orders.FirstAsync();
            Assert.Equal(25m, savedOrder.DiscountAmount);
        }

        // ==================== 優惠券：反向測試 ====================

        [Fact]
        public async Task CreateOrderAsync_WithInvalidCouponCode_ShouldFailAndRollback()
        {
            // Arrange：有效商品，但優惠券代碼不存在
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("NONEXISTENT"));

            // Assert：失敗，訂單未建立，優惠券未被使用
            Assert.False(result.Success);
            Assert.Contains("無效", result.Message);

            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);

            // 驗證無 CouponUsage 記錄
            Assert.Empty(_context.CouponUsages);
        }

        [Fact]
        public async Task CreateOrderAsync_WithExpiredCoupon_ShouldFailAndRollback()
        {
            // Arrange：商品 + 已過期優惠券
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);

            var coupon = new Coupon
            {
                Code = "OLD20",
                Type = CouponType.FixedAmount,
                DiscountValue = 20m,
                Scope = CouponScope.Global,
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(-1), // 已過期
                IsActive = true
            };
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("OLD20"));

            // Assert：失敗，訂單未建立，優惠券未被使用
            Assert.False(result.Success);
            Assert.Contains("無效", result.Message);

            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);

            Assert.Empty(_context.CouponUsages);
        }

        [Fact]
        public async Task CreateOrderAsync_WithInactiveCoupon_ShouldFailAndRollback()
        {
            // Arrange：商品 + 已停用優惠券
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            SeedCoupon("DEAD20", CouponType.FixedAmount, discountValue: 20m, isActive: false);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("DEAD20"));

            // Assert：失敗，訂單未建立，優惠券未被使用
            Assert.False(result.Success);
            Assert.Contains("無效", result.Message);

            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);

            Assert.Empty(_context.CouponUsages);
        }

        [Fact]
        public async Task CreateOrderAsync_WithCouponBelowMinimumOrderAmount_ShouldFailAndRollback()
        {
            // Arrange：商品 $100 × 5 = $500，但優惠券要求最低消費 $1000
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            SeedCoupon("MIN1000", CouponType.FixedAmount, discountValue: 50m,
                minimumOrderAmount: 1000m);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("MIN1000"));

            // Assert：失敗，訂單未建立，優惠券未被使用
            Assert.False(result.Success);
            Assert.Contains("無效", result.Message);

            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);

            Assert.Empty(_context.CouponUsages);
        }

        [Fact]
        public async Task CreateOrderAsync_WithCouponUsageLimitExhausted_ShouldFailAndRollback()
        {
            // Arrange：商品 + 優惠券使用次數已達上限（UsageLimit=1, UsedCount=1）
            SeedProduct(1, stock: 20, price: 100m);
            SeedCartItem(1, productId: 1, quantity: 5);
            SeedCoupon("USEDUP", CouponType.FixedAmount, discountValue: 20m,
                usageLimit: 1, usedCount: 1);

            // Act
            var result = await _service.CreateOrderAsync(1, CreateValidDto("USEDUP"));

            // Assert：失敗，訂單未建立，優惠券未被使用
            Assert.False(result.Success);
            Assert.Contains("無效", result.Message);

            var savedOrder = await _context.Orders.FirstOrDefaultAsync(o => o.UserId == 1);
            Assert.Null(savedOrder);

            Assert.Empty(_context.CouponUsages);
        }
    }
}
