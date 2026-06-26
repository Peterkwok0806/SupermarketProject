using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public interface IProductService
    {

        Task<PagedResultDto<ProductDto>> GetProductsAsync(int? category = null, string? keyword = null, string? sortBy = null, int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<ProductCategory>> GetCategoriesAsync();

        Task<ProductDetailDto?> GetProductByIdAsync(int id);

        Task<PagedResultDto<ProductDto>> GetProductByKeywordAsync(string keyword, int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<string>> GetProductSuggestionsAsync(string query);

        Task<ApiResult> CreateProductAsync(CreateProductDto createProductDto);

        Task<ApiResult> UpdateProductAsync(int id, CreateProductDto createProductDto);

        Task<ApiResult> ToggleAvailabilityAsync(int id);

        /// <summary>
        /// 取得低庫存商品警報統計
        /// </summary>
        /// <param name="threshold">庫存警戒門檻值，預設 10</param>
        /// <returns>低庫存統計資訊（總數 + 前 5 筆最低庫存商品）</returns>
        Task<ApiResult<LowStockAlertDto>> GetLowStockAlertAsync(int threshold = 10);

        /// <summary>
        /// 批量切換商品上架 / 下架狀態
        /// </summary>
        /// <param name="productIds">商品 ID 列表</param>
        /// <param name="isAvailable">目標上架狀態</param>
        /// <returns>操作結果</returns>
        Task<ApiResult> BatchToggleAvailabilityAsync(List<int> productIds, bool isAvailable);

        /// <summary>
        /// 批量軟刪除商品（設定 IsDeleted = true）
        /// </summary>
        /// <param name="productIds">商品 ID 列表</param>
        /// <returns>操作結果</returns>
        Task<ApiResult> BatchSoftDeleteAsync(List<int> productIds);
    }
}
