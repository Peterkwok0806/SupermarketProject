using SupermarketMock.DTOs;

namespace SupermarketMock.IServices
{
    public interface IDashboardService
    {
        Task<ApiResult<DashboardStatsDto>> GetDashboardStatsAsync();

        /// <summary>
        /// 取得最近 N 天的每日銷售趨勢（含零銷量日補齊，總計已扣除取消訂單）
        /// </summary>
        /// <param name="days">查詢天數，預設 7，範圍 1..90</param>
        Task<ApiResult<SalesTrendDto>> GetSalesTrendAsync(int days = 7);
    }
}