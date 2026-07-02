using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly SupermarketContext _context;

        public WishlistService(SupermarketContext context)
        {
            _context = context;
        }

        public async Task<ApiResult> AddToWishlistAsync(int userId, int productId)
        {
            try
            {
                // 檢查商品是否存在
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted);

                if (product == null)
                {
                    return new ApiResult { Success = false, Message = "商品不存在" };
                }

                // 檢查上限：每人最多 50 個
                const int MaxWishlistItems = 50;
                var currentCount = await _context.WishlistItems
                    .CountAsync(w => w.UserId == userId);

                if (currentCount >= MaxWishlistItems)
                {
                    return new ApiResult { Success = false, Message = "您的願望清單已達 50 個上限，請先清理後再加入！" };
                }

                // 檢查是否已收藏
                var exists = await _context.WishlistItems
                    .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

                if (exists)
                {
                    return new ApiResult { Success = false, Message = "此商品已在願望清單中" };
                }

                var wishlistItem = new WishlistItem
                {
                    UserId = userId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WishlistItems.Add(wishlistItem);
                await _context.SaveChangesAsync();

                return new ApiResult { Success = true, Message = "已加入願望清單" };
            }
            catch (Exception ex)
            {
                return new ApiResult { Success = false, Message = $"加入願望清單失敗：{ex.Message}" };
            }
        }

        public async Task<ApiResult> RemoveFromWishlistAsync(int userId, int productId)
        {
            try
            {
                var wishlistItem = await _context.WishlistItems
                    .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

                if (wishlistItem == null)
                {
                    return new ApiResult { Success = false, Message = "此商品不在願望清單中" };
                }

                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();

                return new ApiResult { Success = true, Message = "已從願望清單中移除" };
            }
            catch (Exception ex)
            {
                return new ApiResult { Success = false, Message = $"移除願望清單失敗：{ex.Message}" };
            }
        }

        public async Task<ApiResult<List<ProductDto>>> GetWishlistAsync(int userId)
        {
            // 取得該使用者的所有願望清單項目（含 Product 與 Promotions）
            var wishlistItems = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .OrderByDescending(w => w.CreatedAt)
                .Include(w => w.Product)
                    .ThenInclude(p => p.ProductPromotions)
                        .ThenInclude(pp => pp.Promotion)
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.UtcNow;

            var productDtos = wishlistItems.Select(w =>
            {
                var product = w.Product;

                // 取得目前有效的促銷活動
                var activePromotions = product.ProductPromotions
                    .Where(pp => pp.Promotion.StartDate <= now
                        && pp.Promotion.EndDate >= now)
                    .Select(pp => pp.Promotion)
                    .ToList();

                // 計算折後價格
                var finalPrice = product.Price;
                foreach (var promotion in activePromotions)
                {
                    switch (promotion.Type)
                    {
                        case PromotionType.PercentageOff:
                            finalPrice -= finalPrice * ((promotion.DiscountValue ?? 0) / 100m);
                            break;
                        case PromotionType.FixedDiscount:
                            finalPrice -= promotion.DiscountValue ?? 0;
                            break;
                        case PromotionType.BuyXGetYFree:
                            // 買X送Y 在列表中不改變單價
                            break;
                    }
                }

                finalPrice = Math.Max(0, finalPrice);

                return new ProductDto
                {
                    id = product.Id,
                    snowflakeId = product.SnowflakeId.ToString(),
                    name = product.Name,
                    price = finalPrice,
                    photo = product.Photo,
                    isAvailable = product.IsAvailable,
                    stockQuantity = product.StockQuantity,
                    isOnSale = activePromotions.Any(),
                    originalPrice = activePromotions.Any(p =>
                        p.Type == PromotionType.PercentageOff ||
                        p.Type == PromotionType.FixedDiscount)
                        ? product.Price
                        : null,
                    promotionNames = activePromotions.Select(p => p.Name).ToList()
                };
            }).ToList();

            return new ApiResult<List<ProductDto>>
            {
                Success = true,
                Item = productDtos!
            };
        }

        public async Task<bool> IsInWishlistAsync(int userId, int productId)
        {
            return await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }
    }
}
