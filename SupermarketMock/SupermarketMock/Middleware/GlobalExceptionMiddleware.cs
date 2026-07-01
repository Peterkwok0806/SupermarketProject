using System.Net;
using System.Text.Json;
using SupermarketMock.DTOs;

namespace SupermarketMock.Middleware
{
    /// <summary>
    /// 全域例外處理中介層：攔截所有未處理的例外，
    /// 回傳統一格式的 ApiResult JSON，並記錄錯誤日誌。
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // 記錄完整例外資訊（含堆疊）
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            // 避免重複回寫 Response（例如已在部分寫入狀態下）
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot write error response.");
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";

            // Debug 模式下回傳完整錯誤訊息，Production 則回傳通用訊息
            var message = _env.IsDevelopment()
                ? exception.Message
                : "An internal server error occurred. Please try again later.";

            var result = new ApiResult
            {
                Success = false,
                Message = message
            };

            var json = JsonSerializer.Serialize(result, JsonOptions);
            await context.Response.WriteAsync(json);
        }
    }
}
