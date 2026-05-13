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
            var allProducts = await _productService.GetAllProductsAsync();
            return Ok(allProducts);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }

    }
}
