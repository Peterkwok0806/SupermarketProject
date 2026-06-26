using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Services;
using System.Security.Claims;

namespace SupermarketMock.Controllers
{
    /// <summary>
    /// 顧客端 - 商品評論 API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private int GetCurrentUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : 0;
        }

        // ============================================================
        //  公開 API - 不需登入
        // ============================================================

        /// <summary>取得商品評論列表（公開，僅 Approved）</summary>
        [HttpGet("product/{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviews(
            int productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? rating = null,
            [FromQuery] bool? verifiedOnly = null,
            [FromQuery] bool? hasImage = null,
            [FromQuery] string? sortBy = "newest")
        {
            var filter = new ReviewFilterDto
            {
                Page = page,
                PageSize = pageSize,
                Rating = rating,
                VerifiedOnly = verifiedOnly,
                HasImage = hasImage,
                SortBy = sortBy
            };
            int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            var result = await _reviewService.GetProductReviewsAsync(productId, filter, currentUserId);
            return Ok(result);
        }

        /// <summary>商品評分彙總（公開）</summary>
        [HttpGet("product/{productId}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductReviewStats(int productId)
        {
            var stats = await _reviewService.GetProductReviewStatsAsync(productId);
            return Ok(new ApiResult<ProductReviewStatsDto>
            {
                Success = true,
                Message = "查詢成功",
                Item = stats
            });
        }

        /// <summary>取得單則評論（公開）</summary>
        [HttpGet("{reviewId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReviewById(int reviewId)
        {
            int? currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            var review = await _reviewService.GetReviewByIdAsync(reviewId, currentUserId);
            if (review == null)
                return NotFound(new ApiResult { Success = false, Message = "評論不存在" });
            return Ok(new ApiResult<ReviewDto> { Success = true, Message = "查詢成功", Item = review });
        }

        // ============================================================
        //  需登入 - 使用者操作
        // ============================================================

        /// <summary>建立評論</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.CreateReviewAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>編輯評論（7 天內）</summary>
        [HttpPut("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto dto)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.UpdateReviewAsync(userId, reviewId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>刪除評論（軟刪除）</summary>
        [HttpDelete("{reviewId}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.DeleteReviewAsync(userId, reviewId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>切換點讚</summary>
        [HttpPost("{reviewId}/helpful")]
        [Authorize]
        public async Task<IActionResult> ToggleHelpful(int reviewId)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.ToggleHelpfulAsync(userId, reviewId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>我的評論</summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.GetMyReviewsAsync(userId, page, pageSize);
            return Ok(result);
        }

        /// <summary>檢查我是否可對此商品評論</summary>
        [HttpGet("can-review")]
        [Authorize]
        public async Task<IActionResult> CanReviewProduct([FromQuery] int productId, [FromQuery] int? orderId = null)
        {
            int userId = GetCurrentUserId();
            if (userId <= 0)
                return Unauthorized(new ApiResult { Success = false, Message = "請先登入" });

            var result = await _reviewService.CanReviewProductAsync(userId, productId, orderId);
            return Ok(result);
        }
    }
}