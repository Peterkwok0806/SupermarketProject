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


        public async Task<IEnumerable<Product>> GetProductsAsync(int? category = null)
        {
            var query = _context.Products
                                .Include(p => p.Category)
                                .AsQueryable();

            // 關鍵邏輯：如果 category 是 null，此處會自動跳過，直接查詢全部商品
            if (category.HasValue)
            {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            return await query
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                         .OrderBy(c => c.DisplayOrder)
                         .ToListAsync();
        }
    }
}
