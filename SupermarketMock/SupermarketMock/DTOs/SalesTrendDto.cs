namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 銷售趨勢資料傳輸物件 (用於 Dashboard 圖表)
    /// </summary>
    public class SalesTrendDto
    {
        /// <summary>查詢天數（7 / 14 / 30 ...）</summary>
        public int Days { get; set; }

        /// <summary>起始日期 (yyyy-MM-dd)</summary>
        public string StartDate { get; set; } = string.Empty;

        /// <summary>結束日期 (yyyy-MM-dd)</summary>
        public string EndDate { get; set; } = string.Empty;

        /// <summary>期間內總銷售額（已扣除取消訂單）</summary>
        public decimal TotalSales { get; set; }

        /// <summary>期間內總訂單數（已扣除取消訂單）</summary>
        public int TotalOrders { get; set; }

        /// <summary>每日資料點（伺服器端已補齊零銷量日）</summary>
        public List<SalesTrendPoint> Points { get; set; } = new();
    }

    /// <summary>
    /// 單日銷售資料點
    /// </summary>
    public class SalesTrendPoint
    {
        /// <summary>日期 (yyyy-MM-dd)</summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>當日銷售額</summary>
        public decimal SalesAmount { get; set; }

        /// <summary>當日訂單數</summary>
        public int OrderCount { get; set; }
    }
}