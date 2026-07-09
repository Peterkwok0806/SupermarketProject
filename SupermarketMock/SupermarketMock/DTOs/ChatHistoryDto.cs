namespace SupermarketMock.DTOs
{
    /// <summary>
    /// 聊天歷史服務回傳結果 DTO
    /// </summary>
    public class ChatSessionResultDto
    {
        /// <summary>
        /// Session ID
        /// </summary>
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 是否為新建立的 Session
        /// </summary>
        public bool IsNewSession { get; set; }

        /// <summary>
        /// SK 所需的 ChatHistory 物件序列化成 JSON 字串
        /// </summary>
        public string ChatHistoryJson { get; set; } = "[]";
    }

    /// <summary>
    /// 單筆訊息 DTO
    /// </summary>
    public class ChatMessageDto
    {
        public long Id { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Session 摘要 DTO（用於列表顯示）
    /// </summary>
    public class ChatSessionSummaryDto
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public int MessageCount { get; set; }
        public string? LastMessagePreview { get; set; }
    }
}
