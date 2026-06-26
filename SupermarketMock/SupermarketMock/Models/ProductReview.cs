using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.Models
{
    /// <summary>
    /// 商品評論主表
    /// </summary>
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>
        /// 關聯訂單（用於實購驗證），非實購評論可為 null
        /// </summary>
        public int? OrderId { get; set; }
        public Order? Order { get; set; }

        [Range(1, 5, ErrorMessage = "評分必須介於 1 到 5 顆星")]
        public int Rating { get; set; }

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "評論內容長度需介於 5–2000 字")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 是否實購（由 OrderId 推導後寫入快取）
        /// </summary>
        public bool IsVerifiedPurchase { get; set; }

        /// <summary>
        /// 審核狀態
        /// </summary>
        public ReviewStatus Status { get; set; } = ReviewStatus.Pending;

        public int HelpfulCount { get; set; }

        /// <summary>
        /// 官方回覆
        /// </summary>
        [StringLength(1000)]
        public string? AdminReply { get; set; }

        public DateTime? AdminReplyAt { get; set; }

        public int? AdminReplyUserId { get; set; }

        /// <summary>
        /// 軟刪除
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ReviewImage> Images { get; set; } = new List<ReviewImage>();
        public ICollection<ReviewHelpful> HelpfulVotes { get; set; } = new List<ReviewHelpful>();
    }

    /// <summary>
    /// 評論審核狀態
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>待審核</summary>
        Pending = 0,

        /// <summary>已通過（公開顯示）</summary>
        Approved = 1,

        /// <summary>已拒絕</summary>
        Rejected = 2,

        /// <summary>已隱藏（管理員手動隱藏）</summary>
        Hidden = 3
    }
}