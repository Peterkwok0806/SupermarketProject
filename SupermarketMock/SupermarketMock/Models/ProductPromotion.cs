namespace SupermarketMock.Models
{
    public class ProductPromotion
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int PromotionId { get; set; }
        public Promotion Promotion { get; set; } = null!;

        public DateTime? OverrideStartDate { get; set; }
        public DateTime? OverrideEndDate { get; set; }

        public int Priority { get; set; } = 1;
    }
}
