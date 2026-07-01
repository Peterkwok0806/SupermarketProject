namespace SupermarketMock.Services
{
    /// <summary>
    /// AI 客服服務介面 — 提供超市商品諮詢與聊天功能
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// 根據使用者訊息回覆商品諮詢或一般聊天內容
        /// </summary>
        /// <param name="userMessage">使用者輸入的文字訊息</param>
        /// <returns>AI 產生的回應文字</returns>
        Task<string> GetProductOrChatResponseAsync(string userMessage);
    }
}
