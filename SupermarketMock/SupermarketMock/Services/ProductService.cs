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


        public async Task<PagedResultDto<ProductDto>> GetProductsAsync(int? category = null, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10: pageSize;
            var query = _context.Products.AsQueryable();

            // 關鍵邏輯：如果 category 是 null，此處會自動跳過，直接查詢全部商品
            if (category.HasValue)
            {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            int totalCount = await query.CountAsync();

            var pagedQuery = query
                .OrderBy(p => p.Name) // 分頁前必須排序，否則分頁順序會錯亂
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var productDtos = await BuildProductDtosAsync(pagedQuery);

            return new PagedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ProductCategory>> GetCategoriesAsync()
        {
            return await _context.ProductCategories
                         .OrderBy(c => c.DisplayOrder)
                         .ToListAsync();
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
        {
            var now = DateTime.UtcNow;

            var product = await _context.Products
                            .Include(p => p.Category)
                            .Include(p => p.ProductPromotions
                                .Where(pp => (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                                          && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                                .OrderByDescending(pp => pp.Priority))
                                .ThenInclude(pp => pp.Promotion)
                            .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return null;

            var activePromotions = product.ProductPromotions
                                   .Select(pp => pp.Promotion)
                                   .ToList();

            var primaryPromotion = activePromotions.FirstOrDefault();

            decimal finalPrice = primaryPromotion == null ? product.Price : primaryPromotion.Type switch
            {
            PromotionType.PercentageOff => Math.Round(product.Price * (1 - (primaryPromotion.DiscountValue!.Value / 100)), 2),
            PromotionType.FixedDiscount => Math.Max(0, product.Price - primaryPromotion.DiscountValue!.Value),
            _ => product.Price
            };

            return new ProductDetailDto
            {
                id = product.Id,
                snowflakeId = product.SnowflakeId,
                name = product.Name,
                description = product.Description,
                price = finalPrice, // 最終折後價
                originalPrice = activePromotions.Any(promotion => promotion.Type== PromotionType.PercentageOff|| promotion.Type == PromotionType.FixedDiscount) ? product.Price : null, // 原價
                photo = product.Photo,
                stockQuantity = product.StockQuantity,
                categoryId = product.CategoryId,
                category = product.Category,
                brand = product.Brand,
                weight = product.Weight,
                unit = product.Unit,
                rating = product.Rating,
                reviewCount = product.ReviewCount,
                isOnSale = activePromotions.Any(),
                promotionNames = activePromotions.Select(promotion => promotion.Name).ToList(),
            };

        }

        public async Task<PagedResultDto<ProductDto>> GetProductByKeywordAsync(string keyword, int pageNumber = 1, int pageSize = 10)
        {

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // 檢查關鍵字是否為空，避免全表掃描
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new PagedResultDto<ProductDto>();
            }

            var query = _context.Products.AsNoTracking()
                .Where(p => p.Name.Contains(keyword) ||
                            (p.Description != null && p.Description.Contains(keyword)) ||
                            (p.Brand != null && p.Brand.Contains(keyword)));

            int totalCount = await query.CountAsync();

            var pagedQuery = query
                .OrderBy(p => p.Name) // 分頁前必須排序，否則分頁順序會錯亂
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);


            var productDtos = await BuildProductDtosAsync(pagedQuery);

            return new PagedResultDto<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
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

        private ProductDto CalculateMultipleDiscounts(Product product, List<Promotion> promotions)
        {
            var dto = new ProductDto
            {
                id = product.Id,
                snowflakeId = product.SnowflakeId.ToString(),
                name = product.Name,
                photo = product.Photo,
                isOnSale = promotions.Any(), // 只要有命中活動就是特價中
                originalPrice = promotions.Any(promotion => promotion.Type == PromotionType.PercentageOff || promotion.Type == PromotionType.FixedDiscount) ? product.Price : null,
                promotionNames = promotions.Select(promotion => promotion.Name).ToList(),
            };

            if (!promotions.Any())
            {
                dto.price = product.Price;
                return dto;
            }

            // 權重最高的活動會排在 List 的第一個 (Index 0), 用它來計算最終顯示價格
            var primaryPromotion = promotions.First();

            dto.price = primaryPromotion.Type switch
            {
                PromotionType.PercentageOff =>
                    Math.Round(product.Price * (1 - (primaryPromotion.DiscountValue!.Value / 100)), 2),

                PromotionType.FixedDiscount =>
                    Math.Max(0, product.Price - primaryPromotion.DiscountValue!.Value),

                _ => product.Price
            };

            return dto;

        }

        private async Task<IEnumerable<ProductDto>> BuildProductDtosAsync(IQueryable<Product> query)
        {
            var now = DateTime.UtcNow;

            var rawData = await query
                .Select(p => new
                {
                    Product = p,
                    ActivePromotions = p.ProductPromotions
                        .Where(pp => (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                                  && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                        .OrderByDescending(pp => pp.Priority)
                        .Select(pp => pp.Promotion)
                        .ToList()
                })
                .ToListAsync();

            return rawData
                .Select(item => CalculateMultipleDiscounts(item.Product, item.ActivePromotions))
                .OrderBy(dto => dto.name)
                .ToList();
        }

       

    }
}
