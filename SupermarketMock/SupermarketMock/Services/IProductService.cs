using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public interface IProductService
    {

        Task<PagedResultDto<ProductDto>> GetProductsAsync(int? category = null, int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<ProductCategory>> GetCategoriesAsync();

        Task<ProductDetailDto?> GetProductByIdAsync(int id);

        Task<PagedResultDto<ProductDto>> GetProductByKeywordAsync(string keyword, int pageNumber = 1, int pageSize = 10);

        Task<IEnumerable<string>> GetProductSuggestionsAsync(string query);
    }
}
