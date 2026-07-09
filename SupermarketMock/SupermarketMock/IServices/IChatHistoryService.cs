using SupermarketMock.DTOs;

namespace SupermarketMock.IServices
{
    /// <summary>
    /// 聊天歷史服務介面 — 管理 AI 客服對話的 Session 與訊息持久化
    /// </summary>
    public interface IChatHistoryService
    {
        /// <summary>
        /// 取得或建立 Session，並回傳該 Session 的完整對話歷史（用於傳入 SK）
        /// </summary>
        /// <param name="sessionId">前端傳入的 SessionId，若為 null 則建立新 Session</param>
        /// <param name="userId">已登入使用者的 ID，匿名為 null</param>
        /// <returns>包含 SessionId 與 ChatHistory 的結果</returns>
        Task<ChatSessionResultDto> GetOrCreateSessionAsync(string? sessionId, int? userId);

        /// <summary>
        /// 新增一條訊息到指定 Session
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="role">訊息角色（User / Assistant）</param>
        /// <param name="content">訊息內容</param>
        Task AddMessageAsync(string sessionId, string role, string content);

        /// <summary>
        /// 取得指定 Session 的所有訊息（用於前端顯示歷史）
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        Task<List<ChatMessageDto>> GetMessagesAsync(string sessionId);

        /// <summary>
        /// 刪除指定 Session（軟刪除）
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="userId">擁有者的 UserId（用於權限驗證）</param>
        Task<bool> DeleteSessionAsync(string sessionId, int? userId);

        /// <summary>
        /// 取得使用者的所有 Sessions（僅限已登入使用者）
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        Task<List<ChatSessionSummaryDto>> GetUserSessionsAsync(int userId);

        /// <summary>
        /// 清理過期 Sessions（由 Hangfire 定期呼叫）
        /// </summary>
        Task CleanupExpiredSessionsAsync();
    }
}
