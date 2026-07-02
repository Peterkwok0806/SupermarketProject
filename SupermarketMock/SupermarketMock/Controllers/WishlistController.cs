using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.Services;
using System.Security.Claims;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;

        public WishlistController(IWishlistService wishlistService)
        {
            _wishlistService = wishlistService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        /// <summary>
        /// 取得當前使用者的所有收藏商品
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            int userId = GetCurrentUserId();
            var result = await _wishlistService.GetWishlistAsync(userId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// 加入收藏
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddToWishlist([FromBody] AddToWishlistDto dto)
        {
            try
            {
                if (dto == null || dto.ProductId <= 0)
                {
                    return BadRequest(new { message = "無效的請求資料" });
                }

                int userId = GetCurrentUserId();
                var result = await _wishlistService.AddToWishlistAsync(userId, dto.ProductId);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "加入願望清單失敗",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromWishlist(int productId)
        {
            int userId = GetCurrentUserId();
            var result = await _wishlistService.RemoveFromWishlistAsync(userId, productId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        /// <summary>
        /// 檢查某商品是否已被收藏
        /// </summary>
        [HttpGet("check/{productId}")]
        public async Task<IActionResult> CheckInWishlist(int productId)
        {
            int userId = GetCurrentUserId();
            var isInWishlist = await _wishlistService.IsInWishlistAsync(userId, productId);
            return Ok(new { isInWishlist });
        }
    }

    /// <summary>
    /// 加入願望清單的請求 DTO
    /// </summary>
    public class AddToWishlistDto
    {
        public int ProductId { get; set; }
    }
}
