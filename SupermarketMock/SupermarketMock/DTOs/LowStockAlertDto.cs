namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 低庫存警報統計 DTO
    /// </summary>
    public class LowStockAlertDto
    {
        /// <summary>低庫存商品總數</summary>
        public int TotalLowStockCount { get; set; }

        /// <summary>庫存警戒門檻值</summary>
        public int Threshold { get; set; }

        /// <summary>庫存最低的前 5 筆商品</summary>
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();
    }

    /// <summary>
    /// 低庫存商品簡要 DTO
    /// </summary>
    public class LowStockProductDto
    {
        /// <summary>商品 ID</summary>
        public int Id { get; set; }

        /// <summary>商品名稱</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>目前庫存數量</summary>
        public int StockQuantity { get; set; }
    }
}