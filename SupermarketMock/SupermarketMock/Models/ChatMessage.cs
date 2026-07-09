using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupermarketMock.Models
{
    /// <summary>
    /// AI 客服訊息實體
    /// 用於儲存對話中的每一條訊息（使用者/AI）
    /// </summary>
    [Table("ChatMessages")]
    public class ChatMessage
    {
        /// <summary>
        /// 訊息唯一識別碼
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// 所屬 Session ID
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// 訊息角色：User / Assistant
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// 訊息內容
        /// </summary>
        [Required]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 訊息建立時間
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 導航屬性：所屬的 Session
        /// </summary>
        [ForeignKey(nameof(SessionId))]
        public virtual ChatSession? Session { get; set; }
    }
}
