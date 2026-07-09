namespace SupermarketMock.IServices
{
    /// <summary>
    /// AI 客服服務介面 — 提供超市商品諮詢與聊天功能（支援對話歷史）
    /// </summary>
    public interface IAiChatService
    {
        /// <summary>
        /// 根據使用者訊息回覆商品諮詢或一般聊天內容（無歷史）
        /// </summary>
        /// <param name="userMessage">使用者輸入的文字訊息</param>
        /// <returns>AI 產生的回應文字</returns>
        Task<string> GetProductOrChatResponseAsync(string userMessage);

        /// <summary>
        /// 根據使用者訊息回覆商品諮詢或一般聊天內容（帶對話歷史）
        /// </summary>
        /// <param name="userMessage">使用者輸入的文字訊息</param>
        /// <param name="sessionId">對話 Session ID（null 表示新對話）</param>
        /// <param name="userId">已登入使用者的 ID（匿名為 null）</param>
        /// <returns>AI 產生的回應文字與 Session 資訊</returns>
        Task<AiChatResponseDto> GetProductOrChatResponseWithHistoryAsync(string userMessage, string? sessionId, int? userId);
    }

    /// <summary>
    /// AI 聊天回傳 DTO
    /// </summary>
    public class AiChatResponseDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
    }
}
