using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Get([FromQuery] int? category = null)
        {
            if (category.HasValue)
            {
                var filteredProducts = await _productService.GetProductsAsync(category.Value);
                return Ok(filteredProducts);
            }

            // 沒傳參數就走原來的 GetAllProductsAsync
            var allProducts = await _productService.GetProductsAsync();
            return Ok(allProducts);
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

        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            var products = await _productService.GetProductByKeywordAsync(keyword);
            if (products == null)
            {
                return NotFound(new { message = "找不到相關資源" });
            }

            return Ok(products);
        }

        [HttpGet("suggestions")]
        public async Task<ActionResult<IEnumerable<string>>> GetSuggestions([FromQuery] string q)
        {
                var result = await _productService.GetProductSuggestionsAsync(q);
                return Ok(result);
        }
    }
}
