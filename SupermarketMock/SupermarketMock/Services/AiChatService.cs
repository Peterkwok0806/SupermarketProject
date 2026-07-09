using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SupermarketMock.IServices;

namespace SupermarketMock.Services
{
    /// <summary>
    /// AI 客服服務實作 — 透過 Semantic Kernel 串接 OpenAI / Azure OpenAI，
    /// 結合 EF Core 商品資料庫實現 RAG 搜尋。
    /// </summary>
    public class AiChatService : IAiChatService
    {
        private readonly Kernel _kernel;
        private readonly SupermarketContext _context;
        private readonly IChatHistoryService _chatHistoryService;
        private readonly ILogger<AiChatService> _logger;

        /// <summary>
        /// System Prompt：定義 AI 客服的角色與行為邊界，並告知可使用的工具
        /// </summary>
        private const string SystemPrompt = @"
你是一位專業且親切的【本實體超市門市】線上客服助手。

【鐵律 1：不要詢問顧客城市或地址】
顧客目前已經在我們超市的官方線上商城/App 中與你對話。你不需要、也絕對禁止詢問顧客所在的城市、地址或地區！

【鐵律 2：嚴禁憑空猜測與使用外部常識】
當顧客問你『這裡有沒有賣某商品』或『推薦買什麼』時，你腦海中的外部知識全部都是不可靠的。你必須立刻、馬上呼叫 SearchProductsAsync 工具查詢我們這家超市的即時資料庫！

【鐵律 3：如果工具查不到】
如果使用工具查詢後，發現我們超市真的沒賣該商品，請誠實、客氣地回答：『不好意思，我們這間門市目前沒有販售這款商品喔！』。絕不允許去問顧客住在外面哪裡。";

        /// <summary>
        /// 建構函式：透過 DI 接收 Kernel（由 SK 官方 AddKernel() 自動管理生命週期）
        /// 並複製一份 Kernel 實例，將自己註冊為 Plugin，避免 Captive Dependency 問題。
        /// </summary>
        public AiChatService(
            Kernel kernel, // 👈 由 DI 注入（Transient，每次請求新建）
            SupermarketContext context,
            IChatHistoryService chatHistoryService,
            ILogger<AiChatService> logger)
        {
            _context = context;
            _chatHistoryService = chatHistoryService;
            _logger = logger;

            // 關鍵：複製 Kernel 並將自己註冊為 Plugin
            // 這樣可以確保工具調用時，用的是當前 HTTP 請求的資料庫實例，絕不衝突
            _kernel = kernel.Clone();
            _kernel.Plugins.AddFromObject(this, pluginName: "SupermarketTools");
        }

        /// <summary>
        /// Semantic Kernel Plugin：模糊搜尋超市商品資料庫
        /// AI 會自動判斷使用者意圖並決定是否呼叫此工具
        /// </summary>
        [KernelFunction]
        [Description("【必用工具】只要顧客提到任何商品名稱、或者想要你推薦商品、問有沒有賣時，必須立刻執行此工具查詢超市資料庫。")]
        public async Task<string> SearchProductsAsync(
            [Description("搜尋關鍵字，例如：可樂、飲料、泡麵")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "請提供搜尋關鍵字。";
            }

            var keyword = query.Trim();

            // -------------------------------------------------------
            // 模糊搜尋 Products：匹配 Name、Description、Brand、Category.Name
            // 排除已刪除與下架商品
            // -------------------------------------------------------
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted
                    && (p.Name.Contains(keyword)
                        || (p.Description != null && p.Description.Contains(keyword))
                        || (p.Brand != null && p.Brand.Contains(keyword))
                        || (p.Category != null && p.Category.Name.Contains(keyword))
                        || (p.Category != null && p.Category.Description != null && p.Category.Description.Contains(keyword))))
                .OrderBy(p => p.Name)
                .Take(10)
                .ToListAsync();

            // -------------------------------------------------------
            // 組裝 Markdown 格式回傳
            // -------------------------------------------------------
            if (products.Count == 0)
            {
                return $"🔍 找不到與「{keyword}」相關的商品。建議嘗試其他關鍵字，或聯繫門市人員獲取協助。";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"🔍 搜尋「{keyword}」共找到 {products.Count} 筆商品：\n");

            foreach (var p in products)
            {
                var categoryDisplay = p.Category != null
                    ? $"{p.Category.Icon} {p.Category.Name}（{p.Category.Description ?? ""}）"
                    : "未分類";

                sb.AppendLine($"### 📦 {p.Name}");
                sb.AppendLine($"- **價格**：NT${p.Price:F2}");
                sb.AppendLine($"- **庫存**：{p.StockQuantity} {p.Unit ?? "件"}");
                sb.AppendLine($"- **分類**：{categoryDisplay}");

                if (!string.IsNullOrWhiteSpace(p.Brand))
                    sb.AppendLine($"- **品牌**：{p.Brand}");

                if (p.Weight.HasValue)
                    sb.AppendLine($"- **重量**：{p.Weight} {p.Unit ?? ""}");

                if (!string.IsNullOrWhiteSpace(p.Description))
                    sb.AppendLine($"- **說明**：{p.Description}");

                sb.AppendLine($"- **上架狀態**：{(p.IsAvailable ? "✅ 在售" : "❌ 暫停販售")}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        public async Task<string> GetProductOrChatResponseAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "請輸入您的問題，我很樂意為您服務！";
            }

            try
            {
                var chatService = _kernel.GetRequiredService<IChatCompletionService>();

                // ✨ Kernel.Plugins 已由建構函式註冊，此處無需重複處理

                var chatHistory = new ChatHistory(SystemPrompt);
                chatHistory.AddUserMessage(userMessage);

                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 1024,
                    TopP = 0.9,
                    // 啟用自動插件調用：SK 會根據對話意圖自動決定是否呼叫 SearchProductsAsync
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel);

                return response?.Content ?? "抱歉，我暫時無法回覆您的問題，請稍後再試。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 客服服務呼叫失敗");
                return "抱歉，系統發生錯誤，請稍後再試。若問題持續發生，請聯繫客服人員。";
            }
        }

        /// <inheritdoc/>
        public async Task<AiChatResponseDto> GetProductOrChatResponseWithHistoryAsync(string userMessage, string? sessionId, int? userId)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return new AiChatResponseDto
                {
                    SessionId = sessionId ?? string.Empty,
                    Response = "請輸入您的問題，我很樂意為您服務！"
                };
            }

            try
            {
                // 1. 取得或建立 Session
                var sessionResult = await _chatHistoryService.GetOrCreateSessionAsync(sessionId, userId);

                // 2. 將 SK ChatHistory JSON 反序列化
                var chatHistory = JsonSerializer.Deserialize<ChatHistory>(sessionResult.ChatHistoryJson)
                    ?? new ChatHistory(SystemPrompt);

                // 3. 如果是新 Session，加入 System Prompt
                if (sessionResult.IsNewSession && chatHistory.Count == 0)
                {
                    chatHistory = new ChatHistory(SystemPrompt);
                }

                // 4. 加入使用者訊息
                chatHistory.AddUserMessage(userMessage);

                // 5. 取得 ChatCompletion 服務
                var chatService = _kernel.GetRequiredService<IChatCompletionService>();

                // 6. 執行 SK 生成回覆
                var executionSettings = new OpenAIPromptExecutionSettings
                {
                    Temperature = 0.7,
                    MaxTokens = 1024,
                    TopP = 0.9,
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
                };

                var response = await chatService.GetChatMessageContentAsync(
                    chatHistory,
                    executionSettings,
                    _kernel);

                var responseContent = response?.Content ?? "抱歉，我暫時無法回覆您的問題，請稍後再試。";

                // 7. 儲存使用者訊息與 AI 回覆到資料庫
                await _chatHistoryService.AddMessageAsync(sessionResult.SessionId, "User", userMessage);
                await _chatHistoryService.AddMessageAsync(sessionResult.SessionId, "Assistant", responseContent);

                return new AiChatResponseDto
                {
                    SessionId = sessionResult.SessionId,
                    Response = responseContent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 客服服務呼叫失敗 (SessionId: {SessionId})", sessionId);
                return new AiChatResponseDto
                {
                    SessionId = sessionId ?? string.Empty,
                    Response = "抱歉，系統發生錯誤，請稍後再試。若問題持續發生，請聯繫客服人員。"
                };
            }
        }
    }
}
