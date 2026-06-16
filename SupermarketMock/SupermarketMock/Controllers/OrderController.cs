using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SupermarketMock.Services;
using System.Security.Claims;
using SupermarketMock.DTOs;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            int userId = GetCurrentUserId();
            var result = await _orderService.CreateOrderAsync(userId, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{ordersnowflakeId}")]
        public async Task<IActionResult> GetOrder(string orderSnowflakeId)
        {
            int userId = GetCurrentUserId();
            var order = await _orderService.GetOrderByIdAsync(orderSnowflakeId, userId);
            return order != null ? Ok(order) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            int userId = GetCurrentUserId();
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            return Ok(orders);
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResultPagination<OrderDto>>> SearchOrder(
            [FromQuery] string? snowflakeId,
            [FromQuery] string? userName,
            [FromQuery] string? startDate,
            [FromQuery] string? endDate,
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize)
        {
            var result = await _orderService.SearchOrderAsync(snowflakeId, userName, startDate, endDate, pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
