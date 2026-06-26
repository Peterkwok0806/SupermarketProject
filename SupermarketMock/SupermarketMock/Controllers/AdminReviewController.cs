using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using SupermarketMock.Services;
using System.Security.Claims;

namespace SupermarketMock.Controllers
{
    /// <summary>
    /// 後台 - 評論管理 API
    /// </summary>
    [Route("api/admin/reviews")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public AdminReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetCurrentAdminId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : 0;
        }

        // ============================================================
        //  列表 / 搜尋
        // ============================================================

        /// <summary>後台評論列表（支援多條件篩選）</summary>
        [HttpGet]
        public async Task<IActionResult> GetReviews(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] ReviewStatus? status = null,
            [FromQuery] int? productId = null,
            [FromQuery] int? rating = null,
            [FromQuery] string? keyword = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var filter = new AdminReviewFilterDto
            {
                Page = page,
                PageSize = pageSize,
                Status = status,
                ProductId = productId,
                Rating = rating,
                Keyword = keyword,
                FromDate = fromDate,
                ToDate = toDate
            };
            var result = await _reviewService.AdminGetReviewsAsync(filter);
            return Ok(result);
        }

        /// <summary>後台 - 評論儀表板彙總</summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var data = await _reviewService.AdminGetDashboardAsync();
            return Ok(new ApiResult<ReviewDashboardDto>
            {
                Success = true,
                Message = "查詢成功",
                Item = data
            });
        }

        /// <summary>後台 - 取得單則評論</summary>
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null)
                return NotFound(new ApiResult { Success = false, Message = "評論不存在" });
            return Ok(new ApiResult<ReviewDto> { Success = true, Message = "查詢成功", Item = review });
        }

        // ============================================================
        //  審核 / 操作
        // ============================================================

        /// <summary>變更評論狀態（核可 / 拒絕 / 隱藏）</summary>
        [HttpPut("{reviewId}/status")]
        public async Task<IActionResult> UpdateStatus(int reviewId, [FromBody] AdminUpdateReviewStatusDto dto)
        {
            int adminId = GetCurrentAdminId();
            var result = await _reviewService.AdminUpdateStatusAsync(reviewId, dto.Status, adminId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>官方回覆</summary>
        [HttpPut("{reviewId}/reply")]
        public async Task<IActionResult> Reply(int reviewId, [FromBody] AdminReplyReviewDto dto)
        {
            int adminId = GetCurrentAdminId();
            var result = await _reviewService.AdminReplyAsync(reviewId, dto.Reply, adminId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>刪除評論（軟刪除）</summary>
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var result = await _reviewService.AdminDeleteAsync(reviewId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }

    /// <summary>後台更新狀態請求</summary>
    public class AdminUpdateReviewStatusDto
    {
        public ReviewStatus Status { get; set; }
    }

    /// <summary>後台回覆請求</summary>
    public class AdminReplyReviewDto
    {
        public string Reply { get; set; } = string.Empty;
    }
}