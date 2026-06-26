using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.Models
{
    /// <summary>
    /// 評論附圖
    /// </summary>
    public class ReviewImage
    {
        public int Id { get; set; }

        public int ReviewId { get; set; }
        public ProductReview Review { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public int SortOrder { get; set; } = 0;
    }
}