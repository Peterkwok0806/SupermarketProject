using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public class ProductService : IProductService
    {
        private readonly SupermarketContext _context;

        public ProductService(SupermarketContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsAsync([FromQuery] int? category = null)
        {
            var query = _context.Products.AsQueryable();

            if (category.HasValue)
            {
                // 確保將 int? 轉型為你的 Enum 型別（假設型別名稱為 ProductCategory）
                query = query.Where(p => p.Category == (ProductCategory)category.Value);
            }

            return await query
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public  Task<IEnumerable<int>> GetCategoriesAsync()
        {
            var categories = Enum.GetValues<ProductCategory>()
                         .Cast<int>()
                         .ToList();

            return Task.FromResult<IEnumerable<int>>(categories);
        }
    }
}
