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
    }
}
