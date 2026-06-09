using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Services;
using System.Security.Claims;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(result);

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAndRegister([FromBody] VerifyCodeDto dto)
        {

            var result = await _authService.VerifyAndRegisterAsync(dto);

            if (!result.success)
            {
                return BadRequest(new { message = result.message });
            }

            return Ok(result);
        }
    

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);

                if (!result.success)
                {
                    return BadRequest(new { message = result.message });
                }

                if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpiryTime.HasValue)
                {
                    // 傳入 Token 與時間
                    SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryTime.Value);
                }

                return Ok(new
                {
                    success = result.success,
                    message = result.message,
                    token = result.token,       
                    userdto = result.userdto
                });
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "登入失敗", error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // 從客戶端自動帶過來的 Cookie 中讀取加密的 Refresh Token
            if (!Request.Cookies.TryGetValue("refreshToken", out var oldRefreshToken))
            {
                return Unauthorized(new { message = "找不到刷新憑證" });
            }

            var result = await _authService.RefreshTokenAsync(oldRefreshToken);

            if (!result.success)
            {
                return Unauthorized(new { message = result.message });
            }

            if (!string.IsNullOrEmpty(result.RefreshToken) && result.RefreshTokenExpiryTime.HasValue)
            {
                SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiryTime.Value);
            }

            // 只回傳全新的 Access Token 給前端 LocalStorage
            return Ok(new
            {
                success = result.success,
                message = result.message,
                token = result.token,
                userdto = result.userdto
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // 透過將過期時間設定為「過去的某一秒」，強制瀏覽器立刻銷毀該 Cookie
            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(-1)
            });

            return Ok(new { success = true, message = "已安全登出並清除憑證" });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            int userId = GetCurrentUserId(); 
            var result = await _authService.UpdateProfileAsync(userId, dto);
            return result.success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            int userId = GetCurrentUserId();
            var result = await _authService.ChangePasswordAsync(userId, dto);
            return result.success ? Ok(result) : BadRequest(result);
        }

        private void SetRefreshTokenCookie(string refreshToken, DateTime expiryTime)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,     
                Secure = true,          
                SameSite = SameSiteMode.Lax, 
                Expires = expiryTime
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
