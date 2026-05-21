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

        public async Task<IEnumerable<ProductDto>> GetProductByKeywordAsync(string keyword)
        {
            // 1. 檢查關鍵字是否為空，避免全表掃描
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Enumerable.Empty<ProductDto>();
            }

            var products = await _context.Products
                            .AsNoTracking()
                            .Where(p => p.Name.Contains(keyword) ||
                                        (p.Description != null && p.Description.Contains(keyword)) || 
                                        (p.Brand != null && p.Brand.Contains(keyword)))
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

        public async Task<IEnumerable<string>> GetProductSuggestionsAsync(string query)
        {
            // 核心邏輯處理
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<string>();
            }

            var searchTerm = query.Trim().ToLower();

            // 效能優化：只撈取 Name 欄位並限制 8 筆
            return await _context.Products
                .Where(p => p.Name.ToLower().Contains(searchTerm))
                .Select(p => p.Name)
                .Distinct()
                .Take(8)
                .ToListAsync();
        }

    }
}
