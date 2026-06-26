namespace SupermarketMock.Models
{
    /// <summary>
    /// 評論點讚記錄（複合主鍵 UserId + ReviewId）
    /// </summary>
    public class ReviewHelpful
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ReviewId { get; set; }
        public ProductReview Review { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}