namespace SupermarketMock.Models
{
    public class CouponCategory
    {
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; } = null!;
        public int CategoryId { get; set; }
        public ProductCategory Category { get; set; } = null!;
    }
}