using System.ComponentModel.DataAnnotations;
using SupermarketMock.Models;

namespace SupermarketMock.DTOs
{
    // ===== Admin CRUD DTOs =====

    public class CreateCouponDto
    {
        [Required(ErrorMessage = "優惠碼為必填")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "優惠碼長度需為 2-50 字元")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "優惠碼只能包含英文字母、數字、底線和連字號")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "描述最多 500 字元")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "折扣類型為必填")]
        public CouponType Type { get; set; }

        [Required(ErrorMessage = "折扣值為必填")]
        [Range(0.01, 999999.99, ErrorMessage = "折扣值必須介於 0.01 到 999999.99")]
        public decimal DiscountValue { get; set; }

        [Range(0, 999999.99, ErrorMessage = "最低消費門檻不能為負數")]
        public decimal? MinimumOrderAmount { get; set; }

        [Range(0, 999999.99, ErrorMessage = "最大折扣金額不能為負數")]
        public decimal? MaximumDiscountAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "使用次數限制必須至少為 1")]
        public int? UsageLimit { get; set; }

        [Range(1, 100, ErrorMessage = "每人使用限制必須介於 1 到 100")]
        public int? UsageLimitPerUser { get; set; }

        [Required(ErrorMessage = "適用範圍為必填")]
        public CouponScope Scope { get; set; } = CouponScope.Global;
        public List<int>? ProductIds { get; set; }
        public List<int>? CategoryIds { get; set; }

        [Required(ErrorMessage = "開始日期為必填")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "結束日期為必填")]
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCouponDto : CreateCouponDto
    {
        public int Id { get; set; }
    }

    public class CouponListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CouponType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public int? UsageLimitPerUser { get; set; }
        public CouponScope Scope { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Related IDs for scoped coupons
        public List<int>? ProductIds { get; set; }
        public List<int>? CategoryIds { get; set; }
    }

    // ===== Customer-Facing DTOs =====

    public class ValidateCouponRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderSubtotal { get; set; }
        public List<int>? CartProductIds { get; set; }
        public List<int>? CartCategoryIds { get; set; }
    }

    // Alternative DTO that accepts nullable ints in lists for more resilient deserialization
    public class ValidateCouponRequestFlexibleDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderSubtotal { get; set; }
        public List<int?>? CartProductIds { get; set; }
        public List<int?>? CartCategoryIds { get; set; }

        public ValidateCouponRequestDto ToStrictDto() => new ValidateCouponRequestDto
        {
            Code = Code,
            OrderSubtotal = OrderSubtotal,
            CartProductIds = CartProductIds?.Where(x => x.HasValue).Select(x => x.Value).ToList(),
            CartCategoryIds = CartCategoryIds?.Where(x => x.HasValue).Select(x => x.Value).ToList()
        };
    }

    public class CouponValidationResultDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public int? CouponId { get; set; }
        public string? Code { get; set; }
        public CouponType? Type { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Description { get; set; }
    }

    public class ApplyCouponRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public int OrderId { get; set; }
    }

    public class CouponUsageDto
    {
        public int Id { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public string? CouponDescription { get; set; }
        public CouponType CouponType { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime UsedAt { get; set; }
        public int OrderId { get; set; }
    }

    // ===== Dashboard Stats =====

    public class CouponStatsDto
    {
        public int TotalCoupons { get; set; }
        public int ActiveCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
        public int TotalRedemptions { get; set; }
        public decimal TotalDiscountGiven { get; set; }
    }
}