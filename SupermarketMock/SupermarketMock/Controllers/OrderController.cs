using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SupermarketMock.Services;
using System.Security.Claims;
using SupermarketMock.DTOs;
using SupermarketMock.Models;

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
        public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            int userId = GetCurrentUserId();
            var result = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize);
            return Ok(result);
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

        [HttpPut("{ordersnowflakeId}/status")]
        public async Task<ActionResult<ApiResult>> UpdateOrderStatus(string orderSnowflakeId, [FromBody] OrderStatus newStatus)
        {
            var result = await _orderService.UpdateOrderStatusAsync(orderSnowflakeId, newStatus);
            return result.Success ? Ok(result):BadRequest(result);  
        }
    }
}
