using System.ComponentModel.DataAnnotations;

namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 批量操作請求 DTO
    /// </summary>
    public class BatchOperationRequest
    {
        /// <summary>
        /// 要操作的商品 ID 列表
        /// </summary>
        [Required(ErrorMessage = "商品 ID 列表不得為空")]
        [MinLength(1, ErrorMessage = "至少需要提供一個商品 ID")]
        [MaxLength(500, ErrorMessage = "單次操作最多 500 個商品 ID")]
        public List<int> ProductIds { get; set; } = new();

        /// <summary>
        /// 目標上架狀態（僅批量上下架時使用）
        /// </summary>
        public bool IsAvailable { get; set; }
    }
}