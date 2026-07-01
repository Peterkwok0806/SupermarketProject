using System.ComponentModel;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

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
        private readonly ILogger<AiChatService> _logger;

        /// <summary>
        /// System Prompt：定義 AI 客服的角色與行為邊界，並告知可使用的工具
        /// </summary>
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

        public AiChatService(
            IConfiguration configuration,
            SupermarketContext context,
            ILogger<AiChatService> logger)
        {
            _context = context;
            _logger = logger;

            // -------------------------------------------------------
            // 從 appsettings.json 讀取 AI 設定
            // 支援 OpenAI 直連 與 Azure OpenAI 兩種模式
            // -------------------------------------------------------
            var aiSettings = configuration.GetSection("AzureOpenAI");
            var serviceId = aiSettings["ServiceId"] ?? "chat";
            var modelId = aiSettings["ModelId"] ?? "gpt-4o-mini";
            var endpoint = aiSettings["Endpoint"];
            var apiKey = aiSettings["ApiKey"];
            var deploymentName = aiSettings["DeploymentName"];

            // -------------------------------------------------------
            // 使用 Kernel.CreateBuilder() 初始化 Semantic Kernel
            // -------------------------------------------------------
            var builder = Kernel.CreateBuilder();

            if (!string.IsNullOrWhiteSpace(deploymentName) && !string.IsNullOrWhiteSpace(endpoint))
            {
                // Azure OpenAI 模式（推薦用於正式環境）
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: deploymentName,
                    endpoint: endpoint,
                    apiKey: apiKey ?? string.Empty,
                    serviceId: serviceId);
            }
#pragma warning disable SKEXP0010 // AddOpenAIChatCompletion(Uri) 為實驗性 API，對接自訂 endpoint 必須使用
            else if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
            {
                // OpenAI 直連模式（開發 / 測試用）— 對接 dbai.click 等 OpenAI 相容中轉站
                builder.AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey,
                    endpoint: new Uri(endpoint),
                    serviceId: serviceId);
            }
            else
            {
                _logger.LogWarning(
                    "AI 設定未完整配置，AiChatService 將無法正常運作。" +
                    "請在 appsettings.json 的 AzureOpenAI 區段設定 Endpoint、ApiKey 與 DeploymentName。");
            }

            _kernel = builder.Build();
        }

        /// <summary>
        /// Semantic Kernel Plugin：模糊搜尋超市商品資料庫
        /// AI 會自動判斷使用者意圖並決定是否呼叫此工具
        /// </summary>
        [KernelFunction]
        [Description("根據關鍵字模糊搜尋超市商品與分類，回傳商品名稱、價格、庫存、分類、品牌等資訊。適用於顧客詢問商品相關問題時使用。")]
        public async Task<string> SearchProductsAsync(
            [Description("搜尋關鍵字，可輸入商品名稱、描述、品牌或分類名稱")] string query)
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

                // ✨ 執行期動態加入自己作為 Plugin（避免建構期 DI 參數未就緒的問題）
                if (!_kernel.Plugins.Any(p => p.Name == nameof(AiChatService)))
                {
                    _kernel.Plugins.AddFromObject(this, nameof(AiChatService));
                }

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
    }
}
