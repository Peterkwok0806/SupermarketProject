using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.IServices;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using Microsoft.AspNetCore.Authorization;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResult<DashboardStatsDto>>> GetStats()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// 取得最近 N 天的每日銷售趨勢（用於 Dashboard 折線圖，僅限 Admin）
        /// </summary>
        /// <param name="days">查詢天數，預設 7，範圍 1..90</param>
        /// <returns>銷售趨勢 DTO（含零銷量日補齊）</returns>
        [HttpGet("sales-trend")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<SalesTrendDto>>> GetSalesTrend([FromQuery] int days = 7)
        {
            var result = await _dashboardService.GetSalesTrendAsync(days);
            return Ok(result);
        }
    }
}
