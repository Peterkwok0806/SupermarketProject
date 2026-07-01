using IdGen;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using System.Reflection.Emit;
namespace SupermarketMock.Services
{
    public class OrderService : IOrderService
    {
        private readonly SupermarketContext _context;
        private readonly IIdGenerator<long> _idGenerator;

        public OrderService(SupermarketContext context, IIdGenerator<long> idGenerator)
        {
            _context = context;
            _idGenerator = idGenerator;
        }

        private OrderDto MapToOrderDto(Order order, Dictionary<int, Product> lockedProducts)
        {
            var subTotal = order.OrderItems.Sum(oi => oi.SubTotal);
            return new OrderDto
            {
                snowflakeId = order.SnowflakeId.ToString(),
                totalAmount = order.TotalAmount,
                status = order.Status,
                fullName = order.FullName,
                phone = order.Phone,
                address = order.Address,
                remark = order.Remark,
                createdAt = order.CreatedAt,
                couponCode = order.Coupon?.Code,
                couponType = order.Coupon?.Type.ToString(),
                discountAmount = order.DiscountAmount,
                subTotal = subTotal,
                orderItems = order.OrderItems.Select(oi=>
                {
                    var currentProduct = lockedProducts[oi.ProductId];
                    return new OrderItemDto
                    {
                        productId = oi.ProductId,
                        productName = currentProduct.Name,
                        productPhoto = currentProduct.Photo,
                        quantity = oi.Quantity,
                        unitPrice = oi.UnitPrice,
                        subTotal = oi.SubTotal
                    };
                }).ToList()
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            var subTotal = order.OrderItems.Sum(oi => oi.SubTotal);
            return new OrderDto
            {
                snowflakeId = order.SnowflakeId.ToString(),
                totalAmount = order.TotalAmount,
                status = order.Status,
                fullName = order.FullName,
                phone = order.Phone,
                address = order.Address,
                remark = order.Remark,
                createdAt = order.CreatedAt,
                couponCode = order.Coupon?.Code,
                couponType = order.Coupon?.Type.ToString(),
                discountAmount = order.DiscountAmount,
                subTotal = subTotal,
                orderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    productId = oi.ProductId,
                    productName = oi.Product.Name, 
                    productPhoto = oi.Product.Photo,
                    quantity = oi.Quantity,
                    unitPrice = oi.UnitPrice,
                    subTotal = oi.SubTotal

                }).ToList()
            };
        }

        public async Task<OrderResult> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
                return new OrderResult { Success = false, Message = "購物車是空的" };

            // 2. 依商品 ID 排序，確保所有執行緒加鎖順序一致，徹底封鎖死鎖（Deadlock）
            var sortedCartItems = cartItems.OrderBy(ci => ci.ProductId).ToList();
            int[] productIds = sortedCartItems.Select(ci => ci.ProductId).ToArray();
            var now = DateTime.UtcNow;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var lockedProducts = new Dictionary<int, Product>();
                

                // 依商品 ID 從小到大，嚴格依序鎖定
                foreach (var pid in productIds)
                {
                    Product? product;
                    if (_context.Database.ProviderName?.Contains("InMemory") == true)
                    {
                        // 測試環境：使用一般查詢（不支援 UPDLOCK）
                        product = await _context.Products
                            .Include(p => p.ProductPromotions
                                .Where(pp => (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                                              && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                                .OrderByDescending(pp => pp.Priority))
                            .ThenInclude(pp => pp.Promotion)
                            .FirstOrDefaultAsync(p => p.Id == pid);
                    }
                    else
                    {
                        // 正式環境（SQL Server）：使用原本的鎖定方式
                        product = await _context.Products
                            .FromSql($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {pid}")
                            .Include(p => p.ProductPromotions
                                .Where(pp => (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                                              && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                                .OrderByDescending(pp => pp.Priority))
                            .ThenInclude(pp => pp.Promotion)
                            .FirstOrDefaultAsync();
                    }

                    if (product != null)
                    {
                        lockedProducts.Add(product.Id, product);
                    }
                }
                // 建立訂單主檔
                var order = new Order
                {
                    UserId = userId,
                    SnowflakeId = _idGenerator.CreateId(),
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    Remark = dto.Remark,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                foreach (var cartItem in sortedCartItems)
                {
                    if (!lockedProducts.TryGetValue(cartItem.ProductId, out var dbProduct))
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult { Success = false, Message = "商品已下架或不存在" };
                    }

                    if (dbProduct.StockQuantity < cartItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult { Success = false, Message = $"商品 {dbProduct.Name} 庫存不足！" };
                    }

                    // 扣減記憶體中的實體庫存
                    dbProduct.StockQuantity -= cartItem.Quantity;

                    // 重新防禦性計價：抓出該商品目前權重最高的活動
                    var primaryPromotion = dbProduct.ProductPromotions
                       .Select(pp => pp.Promotion)
                       .FirstOrDefault();

                    // 使用共享的 PricingCalculator 計算折後單價與項目小計
                    decimal finalUnitPrice = PricingCalculator.CalculateFinalPrice(dbProduct, primaryPromotion);
                    decimal itemSubTotal = PricingCalculator.CalculateItemSubTotal(dbProduct, primaryPromotion, cartItem.Quantity);

                    // 建立訂單明細
                    var orderItem = new OrderItem
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = finalUnitPrice,
                        SubTotal = itemSubTotal
                    };

                    order.OrderItems.Add(orderItem);

                    // 累加每次防禦性計算出的明細總金額
                    totalAmount += itemSubTotal;
                }

                // ===== 優惠券處理 =====
                decimal discountAmount = 0;
                if (!string.IsNullOrWhiteSpace(dto.CouponCode))
                {
                    // 鎖定並驗證優惠券（ROWLOCK + UPDLOCK）
                    var coupon = await ValidateAndLockCouponAsync(
                        dto.CouponCode, userId, totalAmount, sortedCartItems, lockedProducts);

                    if (coupon == null)
                    {
                        await transaction.RollbackAsync();
                        return new OrderResult
                        {
                            Success = false,
                            Message = $"優惠券「{dto.CouponCode}」無效或不適用"
                        };
                    }

                    // 計算折扣金額
                    discountAmount = PricingCalculator.CalculateCouponDiscount(coupon, totalAmount);

                    // 設定訂單優惠券資訊
                    order.CouponId = coupon.Id;
                    order.DiscountAmount = discountAmount;

                    // 記錄優惠券使用紀錄
                    _context.CouponUsages.Add(new CouponUsage
                    {
                        CouponId = coupon.Id,
                        UserId = userId,
                        Order = order,
                        DiscountApplied = discountAmount,
                        UsedAt = DateTime.UtcNow
                    });

                    // 累加使用次數並更新時間戳
                    coupon.UsedCount++;
                    coupon.UpdatedAt = DateTime.UtcNow;
                }

                order.TotalAmount = Math.Round(Math.Max(0, totalAmount - discountAmount), 2);

                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cartItems); // 移除這批購物車項目

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderResult
                {
                    Success = true,
                    Message = "訂單建立成功",
                    Order = MapToOrderDto(order, lockedProducts)
                };
            }
            catch (Exception ex) 
            {
                await transaction.RollbackAsync();
                return new OrderResult { Success = false, Message = "建立訂單失敗：" + ex.Message };
            }

        }

        public async Task<OrderDto?> GetOrderByIdAsync(string orderSnowflakeId, int userId)
        {

            if (!long.TryParse(orderSnowflakeId, out long snowflakeIdLong))
            {
                return null;
            }

            var order = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.SnowflakeId == snowflakeIdLong && o.UserId == userId);
            if (order == null) return null;
            return MapToOrderDto(order);


        }

        public async Task<ApiResultPagination<OrderDto>> SearchOrderAsync(string? snowflakeId, string? userName, string? startDate, string? endDate, int pageNumber, int pageSize) 
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Orders.Include(o => o.Coupon).AsQueryable();

            if (!string.IsNullOrWhiteSpace(snowflakeId))
            {
                if (long.TryParse(snowflakeId, out long snowflakeIdLong))
                {
                    query = query.Where(o => o.SnowflakeId == snowflakeIdLong);
                }
            }

            if (!string.IsNullOrWhiteSpace(userName))
            {
                // 假設你的訂單資料表上有 FullName 或 UserName 欄位
                query = query.Where(o => o.FullName.Contains(userName));
            }

            if (!string.IsNullOrWhiteSpace(startDate))
            {
                if (DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    query = query.Where(o => o.CreatedAt >= parsedStartDate);
                }
            }

            if (!string.IsNullOrWhiteSpace(endDate))
            {
                if (DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    // 將時間推至當天最後一刻，確保涵蓋該日期的所有訂單
                    var endOfDay = parsedEndDate.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(o => o.CreatedAt <= endOfDay);
                }
            }



            int totalCount = await query.CountAsync();

            var orders = await query
                        .OrderByDescending(o => o.CreatedAt)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();



            var items = orders.Select(MapToOrderDto).ToList();


            return new ApiResultPagination<OrderDto>
            {
                Success = true,
                Message = orders.Count == 0 ? "沒有找到符合條件的訂單" : "搜尋成功",
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };


        }

        public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapToOrderDto).ToList();
        }

        public async Task<ApiResult> UpdateOrderStatusAsync(string orderSnowflakeId, OrderStatus newStatus)
        {
            if (!long.TryParse(orderSnowflakeId, out long snowflakeIdLong))
            {
                return  new ApiResult() { Success = false};
            }
            var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.SnowflakeId == snowflakeIdLong);

            if (order == null)
            {
                return new ApiResult() { Success = false, Message = "訂單不存在" };
            }

            // 如果是取消訂單，恢復庫存與優惠券使用次數
            if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                // 1. 批次載入相關商品（避免 N+1：一次性查詢而非 foreach 中逐一 FindAsync）
                var productIds = order.OrderItems.Select(oi => oi.ProductId).ToList();
                var products = await _context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                // 2. 恢復每個訂單明細的商品庫存
                foreach (var item in order.OrderItems)
                {
                    if (products.TryGetValue(item.ProductId, out var product))
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }

                // 3. 恢復優惠券使用紀錄（若有）
                if (order.CouponId.HasValue)
                {
                    var usage = await _context.CouponUsages
                        .FirstOrDefaultAsync(u => u.OrderId == order.Id);
                    if (usage != null)
                    {
                        var coupon = await _context.Coupons.FindAsync(order.CouponId.Value);
                        if (coupon != null)
                        {
                            coupon.UsedCount = Math.Max(0, coupon.UsedCount - 1);
                            coupon.UpdatedAt = DateTime.UtcNow;
                        }
                        _context.CouponUsages.Remove(usage);
                    }
                }
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResult() { Success = true, Message = "訂單狀態更新成功" };
        }

        /// <summary>
        /// 鎖定並驗證優惠券是否可用（ROWLOCK + UPDLOCK）。
        /// </summary>
        /// <param name="couponCode">優惠券代碼</param>
        /// <param name="userId">使用者 ID</param>
        /// <param name="orderSubtotal">訂單折前總金額</param>
        /// <param name="cartItems">已排序的購物車項目（用於 Scope 驗證）</param>
        /// <param name="lockedProducts">已鎖定的商品字典（用於 Scope 驗證）</param>
        /// <returns>驗證通過的 Coupon 實體，否則回傳 null</returns>
        private async Task<Coupon?> ValidateAndLockCouponAsync(
            string couponCode,
            int userId,
            decimal orderSubtotal,
            List<CartItem> cartItems,
            Dictionary<int, Product> lockedProducts)
        {
            var now = DateTime.UtcNow;
            Coupon? coupon;

            if (_context.Database.ProviderName?.Contains("InMemory") == true)
            {
                // 測試環境：使用一般查詢（不支援 UPDLOCK）
                coupon = await _context.Coupons
                    .Include(c => c.CouponProducts)
                    .Include(c => c.CouponCategories)
                    .FirstOrDefaultAsync(c => c.Code == couponCode.ToUpper().Trim());
            }
            else
            {
                // 正式環境（SQL Server）：使用 ROWLOCK + UPDLOCK 鎖定優惠券列
                coupon = await _context.Coupons
                    .FromSql($"SELECT * FROM Coupons WITH (ROWLOCK, UPDLOCK) WHERE Code = {couponCode.ToUpper().Trim()}")
                    .Include(c => c.CouponProducts)
                    .Include(c => c.CouponCategories)
                    .FirstOrDefaultAsync();
            }

            if (coupon == null)
                return null;

            // 基本驗證：是否啟用、是否過期、是否超過有效期
            if (!coupon.IsActive)
                return null;

            if (coupon.StartDate > now)
                return null;

            if (coupon.EndDate < now)
                return null;

            // 全域使用次數限制
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return null;

            // 每人使用次數限制
            if (coupon.UsageLimitPerUser.HasValue)
            {
                var userUsageCount = await _context.CouponUsages
                    .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId);

                if (userUsageCount >= coupon.UsageLimitPerUser.Value)
                    return null;
            }

            // 最低消費門檻
            if (coupon.MinimumOrderAmount.HasValue && orderSubtotal < coupon.MinimumOrderAmount.Value)
                return null;

            // Scope 驗證：依適用範圍檢查購物車商品是否符合
            if (coupon.Scope == CouponScope.Product)
            {
                var couponProductIds = coupon.CouponProducts.Select(cp => cp.ProductId).ToList();
                var cartProductIds = cartItems.Select(ci => ci.ProductId).ToList();

                if (!cartProductIds.Any(id => couponProductIds.Contains(id)))
                    return null;
            }
            else if (coupon.Scope == CouponScope.Category)
            {
                var couponCategoryIds = coupon.CouponCategories.Select(cc => cc.CategoryId).ToList();
                var cartCategoryIds = cartItems
                    .Where(ci => lockedProducts.ContainsKey(ci.ProductId))
                    .Select(ci => lockedProducts[ci.ProductId].CategoryId)
                    .Distinct()
                    .ToList();

                if (!cartCategoryIds.Any(id => couponCategoryIds.Contains(id)))
                    return null;
            }

            return coupon;
        }

    }
}
