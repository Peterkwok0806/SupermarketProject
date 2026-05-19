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

                return Ok(result);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "登入失敗", error = ex.Message });
            }
            


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


    }
}
