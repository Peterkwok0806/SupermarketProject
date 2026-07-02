using SupermarketMock.DTOs;

namespace SupermarketMock.Services
{
    public interface IWishlistService
    {
        Task<ApiResult> AddToWishlistAsync(int userId, int productId);
        Task<ApiResult> RemoveFromWishlistAsync(int userId, int productId);
        Task<ApiResult<List<ProductDto>>> GetWishlistAsync(int userId);
        Task<bool> IsInWishlistAsync(int userId, int productId);
    }
}
