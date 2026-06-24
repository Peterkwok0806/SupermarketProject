using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    public class DashboardStatsDto
    {
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public string snowflakeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
