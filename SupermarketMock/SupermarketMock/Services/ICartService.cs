using SupermarketMock.Models;
using SupermarketMock.DTOs;

namespace SupermarketMock.Services
{
    public interface ICartService
    {
        Task<CartDto?> GetCartByUserIdAsync(int userId);
        Task<CartOperationResult> AddToCartAsync(int userId, int productId, int quantity = 1);
        Task<CartOperationResult> UpdateQuantityAsync(int userId, int productId, int quantity);
        Task<CartOperationResult> RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
    }

    public class CartOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CartDto? Cart { get; set; }
    }
}
