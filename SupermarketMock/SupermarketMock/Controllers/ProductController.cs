using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using SupermarketMock.Services;

namespace SupermarketMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProductDto>>> GetProducts(
            [FromQuery] int? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.GetProductsAsync(category, page, pageSize);
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
            var result = await _productService.GetProductByKeywordAsync(keyword,page,pageSize);

            return Ok(result);
        }

        [HttpGet("suggestions")]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestions([FromQuery] string q)
        {
                var result = await _productService.GetProductSuggestionsAsync(q);
                return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResult>> CreateProduct([FromBody] CreateProductDto dto)
        {
            var result = await _productService.CreateProductAsync(dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{productId}")]
        public async Task<ActionResult<ApiResult>> UpdateProduct([FromRoute] int productId, [FromBody] CreateProductDto dto)
        {
            var result = await _productService.UpdateProductAsync(productId, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }
    }   
} 
