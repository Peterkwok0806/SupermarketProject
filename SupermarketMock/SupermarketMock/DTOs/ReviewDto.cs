using System.ComponentModel.DataAnnotations;
using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    // ============================================================
    //  Input DTOs
    // ============================================================

    /// <summary>
    /// 建立評論請求
    /// </summary>
    public class CreateReviewDto
    {
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// 訂單 ID（可選，但若帶入則會進行實購驗證）
        /// </summary>
        public int? OrderId { get; set; }

        [Range(1, 5, ErrorMessage = "評分必須介於 1–5")]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 附圖 URL 列表（最多 5 張，已上傳後取得的 URL）
        /// </summary>
        public List<string> ImageUrls { get; set; } = new();
    }

    /// <summary>
    /// 編輯評論請求（7 天內可編輯）
    /// </summary>
    public class UpdateReviewDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Content { get; set; } = string.Empty;

        public List<string> ImageUrls { get; set; } = new();
    }

    /// <summary>
    /// 商品評論列表篩選
    /// </summary>
    public class ReviewFilterDto
    {
        /// <summary>篩選評分 (1–5)</summary>
        public int? Rating { get; set; }

        /// <summary>僅顯示含圖評論</summary>
        public bool? HasImage { get; set; }

        /// <summary>僅顯示實購評論</summary>
        public bool? VerifiedOnly { get; set; }

        /// <summary>排序: newest | helpful</summary>
        public string? SortBy { get; set; } = "newest";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 後台評論列表篩選
    /// </summary>
    public class AdminReviewFilterDto
    {
        public ReviewStatus? Status { get; set; }
        public int? ProductId { get; set; }
        public int? Rating { get; set; }
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 後台審核請求
    /// </summary>
    public class AdminUpdateReviewStatusDto
    {
        [Required]
        public ReviewStatus Status { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>
    /// 後台官方回覆請求
    /// </summary>
    public class AdminReplyDto
    {
        [Required]
        [StringLength(1000, MinimumLength = 2)]
        public string Reply { get; set; } = string.Empty;
    }

    // ============================================================
    //  Output DTOs
    // ============================================================

    /// <summary>
    /// 評論列表項目
    /// </summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public ReviewStatus Status { get; set; }
        public int HelpfulCount { get; set; }

        public string? AdminReply { get; set; }
        public DateTime? AdminReplyAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<string> ImageUrls { get; set; } = new();

        /// <summary>當前使用者是否已點讚（需登入時填充）</summary>
        public bool HasHelpful { get; set; }
    }

    /// <summary>
    /// 商品評分彙總
    /// </summary>
    public class ProductReviewStatsDto
    {
        public int ProductId { get; set; }
        public int TotalCount { get; set; }
        public double AverageRating { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
        public int VerifiedCount { get; set; }
        public int WithImageCount { get; set; }
    }

    /// <summary>
    /// 我的評論列表項目（含訂單 / 商品資訊）
    /// </summary>
    public class MyReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductPhoto { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public ReviewStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public string? AdminReply { get; set; }
    }

    /// <summary>
    /// 後台審核儀表板
    /// </summary>
    public class ReviewDashboardDto
    {
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int HiddenCount { get; set; }
        public int TodayCount { get; set; }
        public double AverageRating { get; set; }
    }
}