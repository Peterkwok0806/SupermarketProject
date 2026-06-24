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

                    // 計算這款商品的「基礎折後單價」
                    decimal finalUnitPrice = primaryPromotion == null ? dbProduct.Price : primaryPromotion.Type switch
                    {
                        PromotionType.PercentageOff =>
                            Math.Round(dbProduct.Price * (1 - (primaryPromotion.DiscountValue!.Value / 100)), 2),

                        PromotionType.FixedDiscount =>
                            Math.Max(0, dbProduct.Price - primaryPromotion.DiscountValue!.Value),

                        _ => dbProduct.Price // 買二送一、兩件特價時，單件基礎價維持原價
                    };

                    // 計算這款品項的最終明細總額（處理件數組合優惠）
                    decimal itemSubTotal = 0;

                    if (primaryPromotion != null && primaryPromotion.Type == PromotionType.BuyXGetYFree)
                    {
                        int buyQty = primaryPromotion.BuyQuantity!.Value;
                        int freeQty = primaryPromotion.FreeQuantity!.Value;
                        int groupSize = buyQty + freeQty;

                        int completedGroups = cartItem.Quantity / groupSize;
                        int remainder = cartItem.Quantity % groupSize;

                        int chargeableQuantity = (buyQty * completedGroups) + remainder;
                        itemSubTotal = finalUnitPrice * chargeableQuantity;
                    }
                    else if (primaryPromotion != null && primaryPromotion.Type == PromotionType.QuantitySpecialPrice)
                    {
                        int specialQty = primaryPromotion.BuyQuantity!.Value;
                        decimal specialPrice = primaryPromotion.DiscountValue!.Value;

                        int specialGroups = cartItem.Quantity / specialQty;
                        int remainder = cartItem.Quantity % specialQty;

                        itemSubTotal = (specialGroups * specialPrice) + (remainder * finalUnitPrice);
                    }
                    else
                    {
                        // 無數量優惠或無活動，總價 = 基礎折後單價 * 數量
                        itemSubTotal = finalUnitPrice * cartItem.Quantity;
                    }

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

                order.TotalAmount = Math.Round(totalAmount, 2);

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

            var query = _context.Orders.AsQueryable();

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

            // 如果是取消訂單，可以選擇是否恢復庫存（這裡先不恢復，之後可擴展）
            if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                // TODO: 未來可加入恢復庫存的邏輯 
            }

            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResult() { Success = true, Message = "訂單狀態更新成功" };
        }
         


    }
}
