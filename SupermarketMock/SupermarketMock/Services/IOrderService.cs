using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public interface IOrderService
    {
        Task<OrderResult> CreateOrderAsync(int userId, CreateOrderDto dto);
        Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId);
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public OrderDto? Order { get; set; }
    }
}
