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


    }
}
