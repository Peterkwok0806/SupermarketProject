using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;

namespace SupermarketMock.Controllers
{
    [ApiController]
    [Route("api/coupons")]
    [Authorize]
    public class CouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        /// <summary>
        /// 驗證優惠券是否可用
        /// </summary>
        [HttpPost("validate")]
        public async Task<ActionResult<ApiResult<CouponValidationResultDto>>> Validate([FromBody] ValidateCouponRequestFlexibleDto flexibleDto)
        {
            if (flexibleDto == null)
            {
                return BadRequest(new { success = false, message = "Request body is required." });
            }

            // Convert to strict DTO, filtering out any null values from the lists
            var dto = flexibleDto.ToStrictDto();
            var userId = GetCurrentUserId();
            var result = await _couponService.ValidateCouponAsync(dto, userId);

            // Return the full ApiResult wrapper so the frontend can read result.item
            return Ok(result);
        }

        /// <summary>
        /// 將優惠券套用到訂單
        /// </summary>
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _couponService.ApplyCouponToOrderAsync(dto.Code, dto.OrderId, userId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// 取得所有可用優惠券（前台瀏覽用）
        /// </summary>
        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCoupons()
        {
            var result = await _couponService.GetAvailableCouponsAsync();
            return Ok(result);
        }

        /// <summary>
        /// 取得我的優惠券使用紀錄
        /// </summary>
        [HttpGet("usage-history")]
        public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = GetCurrentUserId();
            var result = await _couponService.GetUserCouponHistoryAsync(userId, page, pageSize);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : 0;
        }
    }
}