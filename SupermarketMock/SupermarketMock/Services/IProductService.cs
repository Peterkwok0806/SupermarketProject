using Microsoft.AspNetCore.Mvc;
using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public interface IProductService
    {

        Task<IEnumerable<Product>> GetProductsAsync(int? category = null);

        Task<IEnumerable<ProductCategory>> GetCategoriesAsync();
    }
}
