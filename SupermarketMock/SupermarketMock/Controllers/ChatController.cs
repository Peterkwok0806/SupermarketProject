using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Services;
using System.Security.Claims;

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
        private readonly IChatHistoryService _chatHistoryService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IAiChatService aiChatService,
            IChatHistoryService chatHistoryService,
            ILogger<ChatController> logger)
        {
            _aiChatService = aiChatService;
            _chatHistoryService = chatHistoryService;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/chat
        /// 接收使用者訊息，回傳 AI 客服回覆（支援對話歷史）
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("ai-chat")]
        public async Task<ActionResult<ApiResult<AiChatResponseDto>>> Chat([FromBody] ChatRequestDto request)
        {
            // 基本防呆驗證
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ApiResult<AiChatResponseDto>
                {
                    Success = false,
                    Message = "訊息內容不可為空"
                });
            }

            try
            {
                // 從 JWT claims 取得 UserId（匿名為 null）
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;

                var response = await _aiChatService.GetProductOrChatResponseWithHistoryAsync(
                    request.Message,
                    request.SessionId,
                    userId);

                return Ok(new ApiResult<AiChatResponseDto>
                {
                    Success = true,
                    Message = "success",
                    Item = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI 客服 API 呼叫失敗");

                return StatusCode(500, new ApiResult<AiChatResponseDto>
                {
                    Success = false,
                    Message = "AI 客服暫時無法回應，請稍後再試"
                });
            }
        }

        /// <summary>
        /// GET /api/chat/history/{sessionId}
        /// 取得指定 Session 的聊天歷史
        /// </summary>
        [HttpGet("history/{sessionId}")]
        [EnableRateLimiting("ai-chat")]
        public async Task<ActionResult<ApiResult<List<ChatMessageDto>>>> GetHistory(string sessionId)
        {
            try
            {
                var messages = await _chatHistoryService.GetMessagesAsync(sessionId);
                return Ok(new ApiResult<List<ChatMessageDto>>
                {
                    Success = true,
                    Item = messages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得聊天歷史失敗 (SessionId: {SessionId})", sessionId);
                return StatusCode(500, new ApiResult<List<ChatMessageDto>>
                {
                    Success = false,
                    Message = "取得聊天歷史失敗"
                });
            }
        }

        /// <summary>
        /// GET /api/chat/sessions
        /// 取得當前使用者的所有 Sessions（需登入）
        /// </summary>
        [HttpGet("sessions")]
        [EnableRateLimiting("ai-chat")]
        public async Task<ActionResult<ApiResult<List<ChatSessionSummaryDto>>>> GetSessions()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiResult<List<ChatSessionSummaryDto>>
                    {
                        Success = false,
                        Message = "請先登入才能查看對話記錄"
                    });
                }

                var sessions = await _chatHistoryService.GetUserSessionsAsync(userId);
                return Ok(new ApiResult<List<ChatSessionSummaryDto>>
                {
                    Success = true,
                    Item = sessions
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得 Sessions 失敗");
                return StatusCode(500, new ApiResult<List<ChatSessionSummaryDto>>
                {
                    Success = false,
                    Message = "取得對話列表失敗"
                });
            }
        }

        /// <summary>
        /// DELETE /api/chat/session/{sessionId}
        /// 刪除指定 Session
        /// </summary>
        [HttpDelete("session/{sessionId}")]
        [EnableRateLimiting("ai-chat")]
        public async Task<ActionResult<ApiResult<bool>>> DeleteSession(string sessionId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;
                var result = await _chatHistoryService.DeleteSessionAsync(sessionId, userId);

                if (!result)
                {
                    return NotFound(new ApiResult<bool>
                    {
                        Success = false,
                        Message = "找不到該對話或無權限刪除"
                    });
                }

                return Ok(new ApiResult<bool>
                {
                    Success = true,
                    Item = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除 Session 失敗 (SessionId: {SessionId})", sessionId);
                return StatusCode(500, new ApiResult<bool>
                {
                    Success = false,
                    Message = "刪除對話失敗"
                });
            }
        }
    }
}
