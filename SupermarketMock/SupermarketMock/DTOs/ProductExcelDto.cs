namespace SupermarketMock.DTOs
{
    public class ProductExcelDto
    {
        /// <summary>
        /// 商品名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 商品分類名稱（匯入時用來查找或自動建立 ProductCategory）
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 價格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 庫存量
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string? Description { get; set; }
    }
}
