using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public interface IOrderService
    {
        Task<OrderResult> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<OrderDto?> GetOrderByIdAsync(string orderSnowflakeId, int userId);
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
        Task<ApiResultPagination<OrderDto>> SearchOrderAsync(string? snowflakeId, string? userName, string? startDate, string? endDate, int pageNumber, int pageSize);
        Task<ApiResult> UpdateOrderStatusAsync(string orderSnowflakeId, OrderStatus newStatus);
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderDto? Order { get; set; }
    }
}
