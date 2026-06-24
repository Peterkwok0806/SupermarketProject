using SupermarketMock.DTOs;

namespace SupermarketMock.IServices
{
    public interface IDashboardService
    {
        Task<ApiResult<DashboardStatsDto>> GetDashboardStatsAsync(); 
    }
}
