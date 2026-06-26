namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 熱銷商品 DTO
    /// </summary>
    public class TopSellingProductDto
    {
        /// <summary>排名（1-10）</summary>
        public int Rank { get; set; }

        /// <summary>商品 ID</summary>
        public int ProductId { get; set; }

        /// <summary>商品 Snowflake ID（用於導航）</summary>
        public long SnowflakeId { get; set; }

        /// <summary>商品名稱</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>總銷售數量</summary>
        public int TotalQuantitySold { get; set; }

        /// <summary>總銷售金額</summary>
        public decimal TotalSalesAmount { get; set; }

        /// <summary>商品圖片 URL</summary>
        public string? Photo { get; set; }
    }
}