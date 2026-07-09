using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    /// <summary>
    /// 聊天歷史服務實作 — 管理 AI 客服對話的 Session 與訊息持久化
    /// 包含自動清理機制（已登入使用者最多 5 個 Session）
    /// </summary>
    public class ChatHistoryService : IChatHistoryService
    {
        private readonly SupermarketContext _context;
        private readonly ILogger<ChatHistoryService> _logger;
        private readonly IMemoryCache _cache;

        // 快取 Key 前綴
        private const string CacheKeyPrefix = "chat_session_";
        // Session 有效期（小時）
        private const int SessionExpirationHours = 24;
        // 已登入使用者最多 Session 數
        private const int MaxSessionsPerUser = 5;
        // 單一 Session 最大訊息數
        private const int MaxMessagesPerSession = 50;

        // System Prompt
        private const string SystemPrompt = @"
你是一位專業且友善的超市線上客服助手。你的職責如下：
1. 回答顧客關於商品（品名、價格、庫存、成分、保存期限、促銷活動等）的問題。
2. 提供商品推薦、比價、替代品建議。
3. 說明結帳流程、配送方式、退換貨政策。
4. 對一般聊天保持親切有禮，但始終將話題導回超市購物體驗。

【重要】當顧客詢問商品相關問題時，請先使用 SearchProductsAsync 工具查詢即時商品資料，再根據查詢結果回答。
不要憑空猜測商品資訊，務必以工具查詢到的資料為準。

回答原則：
- 使用繁體中文回答。
- 保持簡潔、精準，避免過度冗長。
- 如果查詢結果中找不到相關商品，請誠實告知並建議顧客聯繫門市確認。
- 絕不編造不存在的商品或價格。";

        public ChatHistoryService(
            SupermarketContext context,
            ILogger<ChatHistoryService> logger,
            IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        /// <inheritdoc/>
        public async Task<ChatSessionResultDto> GetOrCreateSessionAsync(string? sessionId, int? userId)
        {
            // 嘗試從快取取得現有 Session
            var cacheKey = string.IsNullOrEmpty(sessionId) ? null : $"{CacheKeyPrefix}{sessionId}";
            if (!string.IsNullOrEmpty(cacheKey) && _cache.TryGetValue(cacheKey, out ChatSessionResultDto? cached))
            {
                return cached!;
            }

            // 檢查是否需要新建 Session
            var isNewSession = string.IsNullOrEmpty(sessionId) || !await _context.ChatSessions
                .AnyAsync(s => s.SessionId == sessionId && !s.IsDeleted);

            if (isNewSession)
            {
                // 建立新 Session
                sessionId = GenerateSessionId();

                // 已登入使用者：檢查並清理超過 5 個的舊 Session
                if (userId != null)
                {
                    await CleanupOldSessionsForUserAsync(userId.Value);
                }
            }

            // 查詢或建立 Session
            var dbSession = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsDeleted);

            if (dbSession == null)
            {
                dbSession = new ChatSession
                {
                    SessionId = sessionId!,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    LastActivityAt = DateTime.UtcNow
                };
                _context.ChatSessions.Add(dbSession);
                await _context.SaveChangesAsync();
            }

            // 查詢歷史訊息並組裝成 SK ChatHistory
            var messages = await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .Take(MaxMessagesPerSession)
                .ToListAsync();

            var chatHistory = new ChatHistory(SystemPrompt);
            foreach (var msg in messages)
            {
                if (msg.Role == "User")
                {
                    chatHistory.AddUserMessage(msg.Content);
                }
                else if (msg.Role == "Assistant")
                {
                    chatHistory.AddAssistantMessage(msg.Content);
                }
            }

            var result = new ChatSessionResultDto
            {
                SessionId = sessionId!,
                IsNewSession = isNewSession,
                ChatHistoryJson = JsonSerializer.Serialize(chatHistory)
            };

            // 寫入快取（TTL = 30 分鐘）
            _cache.Set(cacheKey ?? $"{CacheKeyPrefix}{sessionId}", result, TimeSpan.FromMinutes(30));

            return result;
        }

        /// <inheritdoc/>
        public async Task AddMessageAsync(string sessionId, string role, string content)
        {
            // 新增訊息
            var message = new ChatMessage
            {
                SessionId = sessionId,
                Role = role,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(message);

            // 更新 Session 的最後活動時間
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsDeleted);

            if (session != null)
            {
                session.LastActivityAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // 清除快取（下次查詢會重新載入）
            var cacheKey = $"{CacheKeyPrefix}{sessionId}";
            _cache.Remove(cacheKey);

            // 檢查訊息數是否超過上限（防禦性：保留最新 50 筆）
            var messageCount = await _context.ChatMessages
                .CountAsync(m => m.SessionId == sessionId);

            if (messageCount > MaxMessagesPerSession)
            {
                // 刪除最舊的訊息（保留 System Prompt + 最新 50 筆）
                var oldestMessages = await _context.ChatMessages
                    .Where(m => m.SessionId == sessionId)
                    .OrderBy(m => m.CreatedAt)
                    .Take(messageCount - MaxMessagesPerSession)
                    .ToListAsync();

                _context.ChatMessages.RemoveRange(oldestMessages);
                await _context.SaveChangesAsync();
            }
        }

        /// <inheritdoc/>
        public async Task<List<ChatMessageDto>> GetMessagesAsync(string sessionId)
        {
            return await _context.ChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteSessionAsync(string sessionId, int? userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && !s.IsDeleted);

            if (session == null)
                return false;

            // 權限檢查：匿名 Session 只能自己刪，否則檢查 UserId
            if (userId == null && session.UserId != null)
            {
                // 嘗試刪除他人 Session：不允許
                return false;
            }

            // 軟刪除
            session.IsDeleted = true;
            session.DeletedAt = DateTime.UtcNow;

            //  Cascade 刪除訊息（EF Core 設定）
            await _context.SaveChangesAsync();

            // 清除快取
            _cache.Remove($"{CacheKeyPrefix}{sessionId}");

            _logger.LogInformation("Chat session {SessionId} deleted by user {UserId}", sessionId, userId?.ToString() ?? "anonymous");
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<ChatSessionSummaryDto>> GetUserSessionsAsync(int userId)
        {
            return await _context.ChatSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderByDescending(s => s.LastActivityAt)
                .Select(s => new ChatSessionSummaryDto
                {
                    SessionId = s.SessionId,
                    CreatedAt = s.CreatedAt,
                    LastActivityAt = s.LastActivityAt,
                    MessageCount = s.Messages.Count,
                    LastMessagePreview = s.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content.Length > 50 ? m.Content.Substring(0, 50) + "..." : m.Content)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task CleanupExpiredSessionsAsync()
        {
            var expirationTime = DateTime.UtcNow.AddHours(-SessionExpirationHours);

            var expiredSessions = await _context.ChatSessions
                .Where(s => !s.IsDeleted && s.LastActivityAt < expirationTime)
                .ToListAsync();

            foreach (var session in expiredSessions)
            {
                session.IsDeleted = true;
                session.DeletedAt = DateTime.UtcNow;

                // 清除快取
                _cache.Remove($"{CacheKeyPrefix}{session.SessionId}");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired chat sessions", expiredSessions.Count);
        }

        /// <summary>
        /// 已登入使用者：清理超過 5 個的舊 Session
        /// </summary>
        private async Task CleanupOldSessionsForUserAsync(int userId)
        {
            var sessions = await _context.ChatSessions
                .Where(s => s.UserId == userId && !s.IsDeleted)
                .OrderBy(s => s.LastActivityAt)
                .ToListAsync();

            if (sessions.Count >= MaxSessionsPerUser)
            {
                // 刪除最舊的 Session（超出上限的部分）
                var toDelete = sessions.Take(sessions.Count - MaxSessionsPerUser + 1);
                foreach (var session in toDelete)
                {
                    session.IsDeleted = true;
                    session.DeletedAt = DateTime.UtcNow;
                    _cache.Remove($"{CacheKeyPrefix}{session.SessionId}");
                }
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up {Count} old chat sessions for user {UserId}", 
                    sessions.Count - MaxSessionsPerUser + 1, userId);
            }
        }

        /// <summary>
        /// 產生全域唯一的 SessionId
        /// </summary>
        private static string GenerateSessionId()
        {
            return $"sess_{Guid.NewGuid():N}_{DateTime.UtcNow:yyyyMMddHHmmss}";
        }
    }
}
