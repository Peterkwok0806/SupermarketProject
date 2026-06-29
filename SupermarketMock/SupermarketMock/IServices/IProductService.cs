using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// 將所有商品匯出為 Excel 檔案 (byte[])
        /// </summary>
        /// <returns>Excel 檔案的二進位內容</returns>
        Task<byte[]> ExportProductsToExcelAsync();

        /// <summary>
        /// 從使用者上傳的 Excel 檔案批次匯入商品
        /// 會根據「分類名稱」自動查找或建立對應的 ProductCategory
        /// </summary>
        /// <param name="file">前端上傳的 .xlsx 檔案</param>
        /// <returns>匯入結果（成功筆數、失敗清單等）</returns>
        Task<ApiResult> ImportProductsFromExcelAsync(IFormFile file);
    }
}
