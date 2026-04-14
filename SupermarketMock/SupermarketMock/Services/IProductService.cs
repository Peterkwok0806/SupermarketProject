using SupermarketMock.Models;
namespace SupermarketMock.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}
