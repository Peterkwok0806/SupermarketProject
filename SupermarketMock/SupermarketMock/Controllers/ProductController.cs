using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using SupermarketMock.Services;
using FluentValidation;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IValidator<CreateProductDto> _createProductValidator;

        public ProductController(IProductService productService, IValidator<CreateProductDto> createProductValidator)
        {
            _productService = productService;
            _createProductValidator = createProductValidator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
            [FromQuery] int? category,
            [FromQuery] string? keyword,
            [FromQuery] string? sortBy,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetProductsAsync(category, keyword, sortBy, page, pageSize);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{productId}")]

        public async Task<IActionResult> GetProductById([FromRoute] int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            return product != null ? Ok(product) : NotFound();

        }

        [HttpGet("search")]

        public async Task<ActionResult<PagedResultDto<ProductDto>>> SearchProducts(
            [FromQuery] string keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetProductByKeywordAsync(keyword, page, pageSize);

            return Ok(result);
        }

        [HttpGet("suggestions")]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestions([FromQuery] string q)
        {
            var result = await _productService.GetProductSuggestionsAsync(q);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult>> CreateProduct([FromForm] CreateProductDto dto)
        {
            var validationResult = await _createProductValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResult
                {
                    Success = false,
                    Message = string.Join("；", validationResult.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _productService.CreateProductAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResult>> UpdateProduct([FromRoute] int productId, [FromForm] CreateProductDto dto)
        {
            var validationResult = await _createProductValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResult
                {
                    Success = false,
                    Message = string.Join("；", validationResult.Errors.Select(e => e.ErrorMessage))
                });
            }

            var result = await _productService.UpdateProductAsync(productId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{productId}/availability")]
        public async Task<ActionResult<ApiResult>> ToggleAvailability([FromRoute] int productId)
        {
            var result = await _productService.ToggleAvailabilityAsync(productId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// 取得低庫存商品警報統計（僅限 Admin）
        /// </summary>
        /// <param name="threshold">庫存警戒門檻值，預設 10</param>
        /// <returns>低庫存統計資訊（總數 + 前 5 筆最低庫存商品）</returns>
        [HttpGet("low-stock-alert")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult<LowStockAlertDto>>> GetLowStockAlert(
            [FromQuery] int threshold = 10)
        {
            var result = await _productService.GetLowStockAlertAsync(threshold);
            return Ok(result);
        }

        /// <summary>
        /// 批量切換商品上架 / 下架狀態（僅限 Admin）
        /// </summary>
        /// <param name="request">批量操作請求（包含商品 ID 列表與目標狀態）</param>
        /// <returns>操作結果</returns>
        [HttpPost("batch/toggle-availability")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult>> BatchToggleAvailability([FromBody] BatchOperationRequest request)
        {
            var result = await _productService.BatchToggleAvailabilityAsync(request.ProductIds, request.IsAvailable);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// 批量軟刪除商品（僅限 Admin）
        /// </summary>
        /// <param name="request">批量操作請求（包含商品 ID 列表）</param>
        /// <returns>操作結果</returns>
        [HttpPost("batch/soft-delete")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult>> BatchSoftDelete([FromBody] BatchOperationRequest request)
        {
            var result = await _productService.BatchSoftDeleteAsync(request.ProductIds);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// 匯出所有商品為 Excel 檔案 (xlsx)（僅限 Admin）
        /// </summary>
        /// <returns>Excel 檔案的 FileResult，下載用</returns>
        [HttpGet("export")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportProducts()
        {
            var fileBytes = await _productService.ExportProductsToExcelAsync();
            var fileName = $"Products_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        /// <summary>
        /// 從 Excel 檔案批次匯入商品（僅限 Admin）
        /// 會根據「商品分類名稱」自動尋找或新建對應的 ProductCategory
        /// </summary>
        /// <param name="file">前端上傳的 .xlsx 檔案 (IFormFile)</param>
        /// <returns>匯入結果 (含成功 / 失敗筆數)</returns>
        [HttpPost("import")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResult>> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResult { Success = false, Message = "請上傳 Excel 檔案" });
            }

            var result = await _productService.ImportProductsFromExcelAsync(file);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
