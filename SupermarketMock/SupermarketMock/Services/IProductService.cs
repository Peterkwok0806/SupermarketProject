using Microsoft.AspNetCore.Mvc;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public interface IProductService
    {

        Task<IEnumerable<ProductDto>> GetProductsAsync(int? category = null);

        Task<IEnumerable<ProductCategory>> GetCategoriesAsync();

        Task<ProductDetailDto?> GetProductByIdAsync(int id);

        Task<IEnumerable<ProductDto>> GetProductByKeywordAsync(string keyword);

        Task<IEnumerable<string>> GetProductSuggestionsAsync(string query);
    }
}
