namespace SupermarketMock.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public CouponType Type { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? MaximumDiscountAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; } = 0;
        public int? UsageLimitPerUser { get; set; }
        public CouponScope Scope { get; set; } = CouponScope.Global;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedByUserId { get; set; }

        // Navigation
        public ICollection<CouponUsage> CouponUsages { get; set; } = new List<CouponUsage>();
        public ICollection<CouponProduct> CouponProducts { get; set; } = new List<CouponProduct>();
        public ICollection<CouponCategory> CouponCategories { get; set; } = new List<CouponCategory>();

        // Computed helpers
        public bool IsExpired => DateTime.UtcNow > EndDate;
        public bool IsUsageExhausted => UsageLimit.HasValue && UsedCount >= UsageLimit.Value;
        public bool IsCurrentlyValid => IsActive && !IsExpired && !IsUsageExhausted
                                        && DateTime.UtcNow >= StartDate;
    }

    public enum CouponType
    {
        Percentage,
        FixedAmount,
        FreeShipping
    }

    public enum CouponScope
    {
        Global,
        Product,
        Category
    }
}