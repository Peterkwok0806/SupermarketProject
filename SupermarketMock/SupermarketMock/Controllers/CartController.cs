using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.Services;
using SupermarketMock.DTOs;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = 1;

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null)
                return NotFound(new { message = "購物車不存在" });



            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            try
            {
                if (dto == null || dto.ProductId <= 0)
                {
                    return BadRequest(new { message = "無效的請求資料" });
                }

                int userId = 1; 

                var result = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);

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
                    message = "加入購物車失敗",
                    error = ex.Message
                });
            }
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto)
        {
            int userId = 1;

            var result = await _cartService.UpdateQuantityAsync(userId, dto.ProductId, dto.Quantity);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        // DELETE: api/cart/remove/{productId} → 移除單一商品
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            int userId = 1;

            var result = await _cartService.RemoveFromCartAsync(userId, productId);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        // DELETE: api/cart/clear → 清空購物車
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            int userId = 1;
            await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "購物車已清空" });
        }

    }
}
