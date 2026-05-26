namespace SupermarketMock.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public PromotionType Type { get; set; }
        public decimal? DiscountValue { get; set; }
        public int? BuyQuantity { get; set; }
        public int? FreeQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();
    }


    public enum PromotionType
    {
        PercentageOff,       
        FixedDiscount,      
        BuyXGetYFree,       
        QuantitySpecialPrice 
    }
}
