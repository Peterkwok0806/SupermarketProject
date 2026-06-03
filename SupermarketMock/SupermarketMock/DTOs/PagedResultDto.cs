namespace SupermarketMock.DTOs
{
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }     // 總共有多少筆商品
        public int PageNumber { get; set; }     // 目前是第幾頁
        public int PageSize { get; set; }       // 每頁抓多少筆
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); // 自動計算總頁數
    }
}
