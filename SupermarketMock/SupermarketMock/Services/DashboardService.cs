using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly SupermarketContext _context;

        public DashboardService(SupermarketContext context)
        {
            _context = context;
        }

        public async Task<ApiResult<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            // 今日訂單數
            var todayOrders = await _context.Orders
                .CountAsync(o => o.CreatedAt.Date == today);

            // 今日收入（排除取消訂單）
            var todayRevenue = await _context.Orders
                .Where(o => o.CreatedAt.Date == today && o.Status != OrderStatus.Cancelled )
                .SumAsync(o => o.TotalAmount);

            // 總商品數
            var totalProducts = await _context.Products.CountAsync();

            // 總用戶數
            var totalUsers = await _context.Users.CountAsync();

            // 待處理訂單
            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == OrderStatus.Pending);

            // 本月收入
            var monthlyRevenue = await _context.Orders
                .Where(o => o.CreatedAt >= firstDayOfMonth && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => o.TotalAmount);

            // 最近 5 筆訂單
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

            var dashboardStatsDto = new DashboardStatsDto()
            {
                TodayOrders = todayOrders,
                TodayRevenue = todayRevenue,
                TotalProducts = totalProducts,
                TotalUsers = totalUsers,
                PendingOrders = pendingOrders,
                MonthlyRevenue = monthlyRevenue,
                RecentOrders = recentOrders
            };

            return new ApiResult<DashboardStatsDto>
            {
                Success = true,
                Item = dashboardStatsDto,
            };
        }
    }
}
