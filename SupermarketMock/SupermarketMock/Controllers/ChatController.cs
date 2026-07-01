using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupermarketMock.DTOs;
using SupermarketMock.Services;

namespace SupermarketMock.Controllers
{
    /// <summary>
    /// AI 客服聊天 API 端點
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IAiChatService aiChatService,
            ILogger<ChatController> logger)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/chat
        /// 接收使用者訊息，回傳 AI 客服回覆
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("ai-chat")]
        public async Task<ActionResult<ApiResult<string>>> Chat([FromBody] ChatRequestDto request)
        {
            // 基本防呆驗證
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ApiResult<string>
                {
                    Success = false,
                    Message = "訊息內容不可為空"
                });
            }

            try
            {
                var response = await _aiChatService.GetProductOrChatResponseAsync(request.Message);

                return Ok(new ApiResult<string>
                {
                    Success = true,
                    Message = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 客服 API 呼叫失敗");

                return StatusCode(500, new ApiResult<string>
                {
                    Success = false,
                    Message = "AI 客服暫時無法回應，請稍後再試"
                });
            }
        }
    }
}
