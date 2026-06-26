using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    /// <summary>
    /// 商品評論服務介面
    /// </summary>
    public interface IReviewService
    {
        // ============ 顧客端 ============

        /// <summary>建立評論</summary>
        Task<ApiResult<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto);

        /// <summary>編輯評論（7 天內）</summary>
        Task<ApiResult<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto);

        /// <summary>刪除評論（軟刪除）</summary>
        Task<ApiResult> DeleteReviewAsync(int userId, int reviewId);

        /// <summary>商品評論分頁列表（公開）</summary>
        Task<ApiResultPagination<ReviewDto>> GetProductReviewsAsync(int productId, ReviewFilterDto filter, int? currentUserId = null);

        /// <summary>商品評分彙總</summary>
        Task<ProductReviewStatsDto> GetProductReviewStatsAsync(int productId);

        /// <summary>我的評論</summary>
        Task<ApiResultPagination<MyReviewDto>> GetMyReviewsAsync(int userId, int page, int pageSize);

        /// <summary>取得單則評論</summary>
        Task<ReviewDto?> GetReviewByIdAsync(int reviewId, int? currentUserId = null);

        /// <summary>切換點讚（已讚取消，否則新增）</summary>
        Task<ApiResult<bool>> ToggleHelpfulAsync(int userId, int reviewId);

        /// <summary>檢查使用者是否可對此商品評論（已購買且未評論過）</summary>
        Task<ApiResult<bool>> CanReviewProductAsync(int userId, int productId, int? orderId = null);

        // ============ 後台 ============

        Task<ApiResultPagination<ReviewDto>> AdminGetReviewsAsync(AdminReviewFilterDto filter);

        Task<ApiResult<ReviewDto>> AdminUpdateStatusAsync(int reviewId, ReviewStatus status, int adminUserId);

        Task<ApiResult<ReviewDto>> AdminReplyAsync(int reviewId, string reply, int adminUserId);

        Task<ApiResult> AdminDeleteAsync(int reviewId);

        Task<ReviewDashboardDto> AdminGetDashboardAsync();
    }
}