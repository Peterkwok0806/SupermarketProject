using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupermarketMock.Models
{
    /// <summary>
    /// AI 客服對話 Session 實體
    /// 用於追蹤每次對話的生命週期與關聯使用者
    /// </summary>
    [Table("ChatSessions")]
    public class ChatSession
    {
        /// <summary>
        /// Session 唯一識別碼（前端產生，全域唯一）
        /// </summary>
        [Key]
        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 關聯的使用者 ID（匿名為 null）
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Session 建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最後一次活動時間（用於清理過期 Session）
        /// </summary>
        public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 是否已軟刪除
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// 軟刪除時間
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// 導航屬性：該 Session 的所有訊息
        /// </summary>
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        /// <summary>
        /// 導航屬性：關聯的使用者
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
