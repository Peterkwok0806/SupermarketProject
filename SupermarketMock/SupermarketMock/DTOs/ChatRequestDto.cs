namespace SupermarketMock.DTOs
{
    /// <summary>
    /// AI 客服聊天請求 DTO
    /// </summary>
    public class ChatRequestDto
    {
        /// <summary>
        /// 使用者輸入的文字訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
