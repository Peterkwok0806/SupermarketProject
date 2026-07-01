using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Models;
using Microsoft.Extensions.Caching.Memory;

namespace SupermarketMock.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SupermarketContext _context;
        private readonly IMemoryCache _cache;

        public DashboardService(SupermarketContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<ApiResult<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var cacheKey = CacheKeys.DashboardStats(today);

            var dashboardStatsDto = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

                var todayOrders = await _context.Orders
                    .CountAsync(o => o.CreatedAt.Date == today);

                var todayRevenue = await _context.Orders
                    .Where(o => o.CreatedAt.Date == today && o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount);

                var totalProducts = await _context.Products.CountAsync();
                var totalUsers = await _context.Users.CountAsync();

                var pendingOrders = await _context.Orders
                    .CountAsync(o => o.Status == OrderStatus.Pending);

                var monthlyRevenue = await _context.Orders
                    .Where(o => o.CreatedAt >= firstDayOfMonth && o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => o.TotalAmount);

                var recentOrders = await _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
                    .Select(o => new RecentOrderDto
                    {
                        snowflakeId = o.SnowflakeId.ToString(),
                        FullName = o.FullName,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt
                    })
                    .ToListAsync();

                return new DashboardStatsDto()
                {
                    TodayOrders = todayOrders,
                    TodayRevenue = todayRevenue,
                    TotalProducts = totalProducts,
                    TotalUsers = totalUsers,
                    PendingOrders = pendingOrders,
                    MonthlyRevenue = monthlyRevenue,
                    RecentOrders = recentOrders
                };
            });

            return new ApiResult<DashboardStatsDto>
            {
                Success = true,
                Item = dashboardStatsDto,
            };
        }

        public async Task<ApiResult<SalesTrendDto>> GetSalesTrendAsync(int days = 7)
        {
            // 1. 輸入驗證：限制在 1..90 天之間，避免過大查詢
            if (days < 1) days = 7;
            if (days > 90) days = 90;

            // 2. 時區處理：資料庫中的 CreatedAt 為 UTC，但營業/使用者期望以「香港時間」日期為準。
            //    使用 TimeZoneInfo 在 UTC 與 HKT 之間雙向轉換，確保跨 UTC 午夜的訂單
            //    被歸入正確的「當地日期」桶 (例如 HKT 00:30 = UTC 16:30 應屬於 HKT 當天)。
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Hong_Kong");
            var endDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;
            var startDate = endDate.AddDays(-(days - 1));

            // 3. 計算 UTC 邊界：用以在資料庫層級做範圍過濾，盡量縮小資料集
            //    (startDateLocal 00:00 HKT -> UTC, endDateLocal+1 00:00 HKT -> UTC)
            var rangeStartUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified), tz);
            var rangeEndUtcExclusive = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Unspecified), tz);

            // 4. 一次查詢：先在 SQL 端以 UTC 邊界做範圍過濾，再在記憶體中以 HKT 日期 GROUP BY
            //    (EF Core 無法直接對 CreatedAt 做 TimeZoneInfo 轉換，必須在 client-side 分組)
            var raw = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= rangeStartUtc
                         && o.CreatedAt < rangeEndUtcExclusive
                         && o.Status != OrderStatus.Cancelled)
                .Select(o => new
                {
                    UtcTime = o.CreatedAt,
                    o.TotalAmount
                })
                .ToListAsync();

            var grouped = raw
                .GroupBy(x => TimeZoneInfo.ConvertTime(x.UtcTime, tz).Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Total = g.Sum(x => x.TotalAmount),
                    Count = g.Count()
                })
                .ToList();

            // 5. 建立查詢結果字典 (key = yyyy-MM-dd)
            var byDate = grouped.ToDictionary(x => x.Date.ToString("yyyy-MM-dd"));

            // 6. 補齊零銷量日：走訪整段區間，確保圖表連續、無缺口
            var points = new List<SalesTrendPoint>(days);
            decimal totalSales = 0m;
            int totalOrders = 0;

            for (int i = 0; i < days; i++)
            {
                var d = startDate.AddDays(i);
                var key = d.ToString("yyyy-MM-dd");

                decimal sales = 0m;
                int count = 0;
                if (byDate.TryGetValue(key, out var found))
                {
                    // 對每日 salesAmount 也做 2 位小數四捨五入，避免顯示超長小數
                    sales = Math.Round(found.Total, 2);
                    count = found.Count;
                }

                points.Add(new SalesTrendPoint
                {
                    Date = key,
                    SalesAmount = sales,
                    OrderCount = count
                });

                totalSales += sales;
                totalOrders += count;
            }

            var result = new SalesTrendDto
            {
                Days = days,
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd"),
                TotalSales = Math.Round(totalSales, 2),
                TotalOrders = totalOrders,
                Points = points
            };

            return new ApiResult<SalesTrendDto>
            {
                Success = true,
                Item = result
            };
        }

        public async Task<ApiResult<List<TopSellingProductDto>>> GetTopSellingProductsAsync()
        {
            // 熱銷商品 30 分鐘快取（使用者對排行榜即時性要求不高）
            var result = await _cache.GetOrCreateAsync(CacheKeys.TopSellingProducts, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var topAggregates = await _context.OrderItems
                    .AsNoTracking()
                    .Where(oi => !oi.Product.IsDeleted
                              && oi.Order.Status != OrderStatus.Cancelled)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantity = g.Sum(oi => oi.Quantity),
                        TotalAmount = g.Sum(oi => oi.SubTotal)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(10)
                    .ToListAsync();

                var productIds = topAggregates.Select(x => x.ProductId).ToList();
                var productLookup = await _context.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                return topAggregates
                    .Select((x, index) =>
                    {
                        productLookup.TryGetValue(x.ProductId, out var product);
                        return new TopSellingProductDto
                        {
                            Rank = index + 1,
                            ProductId = x.ProductId,
                            SnowflakeId = product?.SnowflakeId ?? 0,
                            ProductName = product?.Name ?? "Unknown",
                            TotalQuantitySold = x.TotalQuantity,
                            TotalSalesAmount = Math.Round(x.TotalAmount, 2),
                            Photo = product?.Photo
                        };
                    })
                    .ToList();
            });

            return new ApiResult<List<TopSellingProductDto>>
            {
                Success = true,
                Item = result
            };
        }
    }
}
