using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
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


        public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? category = null)
        {
            var query = _context.Products.AsQueryable();

            // 關鍵邏輯：如果 category 是 null，此處會自動跳過，直接查詢全部商品
            if (category.HasValue)
            {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            var products = await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                photo = p.Photo
            }).ToListAsync();

            return products;
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                         .OrderBy(c => c.DisplayOrder)
                         .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                                .Include(p => p.Category)
                                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
