namespace SupermarketMock.Services
{
    /// <summary>
    /// 集中管理所有快取鍵（Cache Key），避免各處硬編字串導致不一致。
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>所有商品分類（依 DisplayOrder 排序）</summary>
        public const string Categories = "categories_all";

        /// <summary>儀表板統計資料（以日期為單位，例如 dashboard_stats_20260701）</summary>
        public static string DashboardStats(DateTime date) => $"dashboard_stats_{date:yyyyMMdd}";

        /// <summary>熱銷商品 Top 10（30 分鐘快取）</summary>
        public const string TopSellingProducts = "top_sellers";
    }
}
