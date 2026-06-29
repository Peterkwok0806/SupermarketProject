using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Models;

namespace SupermarketMock.Controllers
{
    [ApiController]
    [Route("api/admin/coupons")]
    [Authorize(Roles = "Admin")]
    public class AdminCouponController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public AdminCouponController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        /// <summary>優惠券統計 Dashboard</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _couponService.GetCouponStatsAsync();
            return Ok(stats);
        }

        /// <summary>優惠券列表（分頁、篩選）</summary>
        [HttpGet]
        public async Task<IActionResult> GetCoupons(
            [FromQuery] string? search,
            [FromQuery] CouponType? type,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] string? sort,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _couponService.GetCouponsAsync(search, type, isActive, isExpired, sort, page, pageSize);
            return Ok(result);
        }

        /// <summary>取得單一優惠券</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCoupon(int id)
        {
            var result = await _couponService.GetCouponByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>新增優惠券</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            var adminUserId = GetCurrentUserId();
            var result = await _couponService.CreateCouponAsync(dto, adminUserId);
            if (!result.Success)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetCoupon), new { id = result.Item!.Id }, result);
        }

        /// <summary>修改優惠券</summary>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateCouponDto dto)
        {
            var result = await _couponService.UpdateCouponAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>刪除優惠券</summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _couponService.DeleteCouponAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        /// <summary>批量刪除優惠券</summary>
        [HttpPost("bulk-delete")]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(new { success = false, message = "No coupon IDs provided" });

            int deleted = 0;
            var errors = new List<string>();
            foreach (var id in ids)
            {
                var result = await _couponService.DeleteCouponAsync(id);
                if (result.Success)
                    deleted++;
                else
                    errors.Add($"Coupon #{id}: {result.Message}");
            }

            return Ok(new { success = true, deleted, errors });
        }

        /// <summary>切換優惠券啟用狀態</summary>
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _couponService.ToggleCouponActiveAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var v = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(v, out var id) ? id : 0;
        }
    }
}