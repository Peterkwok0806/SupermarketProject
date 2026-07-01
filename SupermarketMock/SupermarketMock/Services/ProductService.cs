using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using IdGen;
using Microsoft.Extensions.Caching.Memory;

namespace SupermarketMock.Services
{
    public class ProductService : IProductService
    {
        private readonly SupermarketContext _context;
        private readonly IFileUploadService _fileUploadService;
        private readonly IIdGenerator<long> _idGenerator;
        private readonly IMemoryCache _cache;

        public ProductService(SupermarketContext context, IFileUploadService fileUploadService,
            IIdGenerator<long> idGenerator, IMemoryCache cache)
        {
            _context = context;
            _fileUploadService = fileUploadService;
            _idGenerator = idGenerator;
            _cache = cache;
        }


        public async Task<PagedResultDto<ProductDto>> GetProductsAsync(int? category = null, string? keyword = null, string? sortBy = null, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10: pageSize;
            var query = _context.Products.Where(p => !p.IsDeleted).AsQueryable();

            // 關鍵邏輯：如果 category 是 null，此處會自動跳過，直接查詢全部商品
            if (category.HasValue)
            {
                query = query.Where(p => p.CategoryId == category.Value);
            }

            // 搜尋邏輯：根據名稱 / 描述 / 品牌 進行模糊比對
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim();
                query = query.Where(p => p.Name.Contains(kw) ||
                                         (p.Description != null && p.Description.Contains(kw)) ||
                                         (p.Brand != null && p.Brand.Contains(kw)));
            }

            int totalCount = await query.CountAsync();

            // 排序邏輯：支援價格升降冪、名稱升降冪
            IOrderedQueryable<Product> orderedQuery = (sortBy ?? "name_asc").ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var pagedQuery = orderedQuery
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
            return await _cache.GetOrCreateAsync(CacheKeys.Categories, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                return await _context.ProductCategories
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();
            });
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
        {
            var now = DateTime.UtcNow;

            var product = await _context.Products
                            .Where(p => !p.IsDeleted)
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
                .Where(p => !p.IsDeleted &&
                            (p.Name.Contains(keyword) ||
                            (p.Description != null && p.Description.Contains(keyword)) ||
                            (p.Brand != null && p.Brand.Contains(keyword))));

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
                .Where(p => p.Name.ToLower().Contains(searchTerm) && !p.IsDeleted)
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
                isAvailable = product.IsAvailable,
                stockQuantity = product.StockQuantity,
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

            // 1. 先將商品列表撈進記憶體
            var products = await query.ToListAsync();

            if (!products.Any())
                return Enumerable.Empty<ProductDto>();

            // 2. 取得這些商品的 ID，一次查詢所有符合條件的促銷活動
            var productIds = products.Select(p => p.Id).ToList();

            var activePromotionsList = await _context.ProductPromotions
                .Where(pp => productIds.Contains(pp.ProductId)
                          && (pp.OverrideStartDate ?? pp.Promotion.StartDate) <= now
                          && (pp.OverrideEndDate ?? pp.Promotion.EndDate) >= now)
                .OrderByDescending(pp => pp.Priority)
                .Include(pp => pp.Promotion)
                .ToListAsync();

            // 3. 以 ProductId 為 Key 建立 Dictionary，方便快速查找
            var promotionsDict = activePromotionsList
                .GroupBy(pp => pp.ProductId)
                .ToDictionary(g => g.Key, g => g.Select(pp => pp.Promotion).ToList());

            // 4. 組裝 DTO
            return products
                .Select(p => CalculateMultipleDiscounts(p, promotionsDict.GetValueOrDefault(p.Id, new List<Promotion>())))
                .OrderBy(dto => dto.name)
                .ToList();
        }

        public async Task<ApiResult> CreateProductAsync(CreateProductDto dto)
        {
            // 1. 檢查名稱是否重複（排除已軟刪除的商品）
            if (await _context.Products.AnyAsync(p => p.Name == dto.Name && !p.IsDeleted))
            {
                return new ApiResult { Success = false, Message = "已有相同名稱貨品" };
            }

            // 2. 處理圖片上傳邏輯
            string? savedPhotoPath = null;
            if (dto.Photofile != null && dto.Photofile.Length > 0)
            {
                var subFolder = Path.Combine("images", "products");

                // 呼叫圖片服務執行驗證與儲存，成功會回傳新檔名（例如: "abc-123.jpg"）

                var fileName = await _fileUploadService.UploadImageAsync(dto.Photofile, subFolder);

                if (fileName == null)
                {
                    return new ApiResult { Success = false, Message = "圖片上傳失敗，只支援 JPG, PNG, WEBP 格式" };
                }

                savedPhotoPath = $"/{fileName}";
            }
            else
            {
                // 如果管理員沒上傳圖片，可以給一個前端 public 資料夾內的預設圖路徑
                savedPhotoPath = "default-product.jpg";
            }


            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Photo = savedPhotoPath,
                StockQuantity = dto.StockQuantity,
                IsAvailable = true,
                Brand = dto.Brand
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ApiResult { Success = true, Message = "已新增貨品" };

        }

        public async Task<ApiResult> ToggleAvailabilityAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var product = await _context.Products
                    .FromSql($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {id} AND IsDeleted = 0")
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return new ApiResult { Success = false, Message = "找不到貨品" };
                }

                // 切換上架狀態
                product.IsAvailable = !product.IsAvailable;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResult
                {
                    Success = true,
                    Message = product.IsAvailable ? "已上架" : "已下架"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResult { Success = false, Message = "切換上下架失敗：" + ex.Message };
            }
        }

        /// <inheritdoc/>
        public async Task<ApiResult<LowStockAlertDto>> GetLowStockAlertAsync(int threshold = 10)
        {
            // 確保門檻值 >= 1，避免無意義查詢
            if (threshold < 1) threshold = 10;

            // 查詢低庫存商品總數（僅限上架且庫存 > 0，排除已軟刪除），AsNoTracking 提升讀取效能
            var totalLowStockCount = await _context.Products
                .AsNoTracking()
                .CountAsync(p => !p.IsDeleted && p.IsAvailable && p.StockQuantity <= threshold && p.StockQuantity > 0);

            // 查詢庫存最低的前 5 筆商品
            var lowStockProducts = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.IsAvailable && p.StockQuantity <= threshold && p.StockQuantity > 0)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .Select(p => new LowStockProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            return new ApiResult<LowStockAlertDto>
            {
                Success = true,
                Item = new LowStockAlertDto
                {
                    TotalLowStockCount = totalLowStockCount,
                    Threshold = threshold,
                    LowStockProducts = lowStockProducts
                }
            };
        }

        /// <inheritdoc/>
        public async Task<ApiResult> BatchToggleAvailabilityAsync(List<int> productIds, bool isAvailable)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return new ApiResult { Success = false, Message = "未提供商品 ID" };
            }

            if (productIds.Count > 500)
            {
                return new ApiResult { Success = false, Message = "單次操作最多 500 個商品" };
            }

            try
            {
                var affectedRows = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.IsAvailable, isAvailable));

                if (affectedRows == 0)
                {
                    return new ApiResult { Success = false, Message = "找不到符合條件的商品" };
                }

                return new ApiResult
                {
                    Success = true,
                    Message = $"已批量{(isAvailable ? "上架" : "下架")} {affectedRows} 項商品"
                };
            }
            catch (Exception)
            {
                return new ApiResult { Success = false, Message = "批量上下架失敗，請稍後再試" };
            }
        }

        /// <inheritdoc/>
        public async Task<ApiResult> BatchSoftDeleteAsync(List<int> productIds)
        {
            if (productIds == null || productIds.Count == 0)
            {
                return new ApiResult { Success = false, Message = "未提供商品 ID" };
            }

            if (productIds.Count > 500)
            {
                return new ApiResult { Success = false, Message = "單次操作最多 500 個商品" };
            }

            try
            {
                var affectedRows = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.IsDeleted)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(p => p.IsDeleted, true)
                        .SetProperty(p => p.DeletedAt, DateTime.UtcNow));

                if (affectedRows == 0)
                {
                    return new ApiResult { Success = false, Message = "找不到符合條件的商品" };
                }

                return new ApiResult
                {
                    Success = true,
                    Message = $"已成功軟刪除 {affectedRows} 項商品"
                };
            }
            catch (Exception)
            {
                return new ApiResult { Success = false, Message = "批量軟刪除失敗，請稍後再試" };
            }
        }

        public async Task<ApiResult> UpdateProductAsync(int id, CreateProductDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var product = await _context.Products
                    .FromSql($"SELECT * FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Id = {id} AND IsDeleted = 0")
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                    return new ApiResult { Success = false, Message = "找不到貨品" };
                }

                string savePath = product.Photo;

                if (dto.Photofile != null && dto.Photofile.Length > 0)
                {

                    var subFolder = Path.Combine("images", "products");

                    var fileName = await _fileUploadService.UploadImageAsync(dto.Photofile, subFolder);

                    if (fileName == null)
                    {
                        return new ApiResult { Success = false, Message = "圖片上傳失敗，只支援 JPG, PNG, WEBP 格式" };
                    }

                    if (!string.IsNullOrEmpty(product.Photo) && !product.Photo.Contains("default-product.jpg"))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.Photo.TrimStart('/'));
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }

                    savePath = $"/{fileName}";

                }

                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.CategoryId = dto.CategoryId;
                product.Photo = savePath;
                product.StockQuantity = dto.StockQuantity;
                product.Brand = dto.Brand;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ApiResult { Success = true, Message = "已更新貨品" };

            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                return new ApiResult { Success = false, Message = "更新貨品失敗：" + ex.Message };
            }
        }

        /// <summary>
        /// 將所有未軟刪除商品匯出為 Excel (xlsx) 檔案。
        /// 使用 EPPlus 8.x 寫入，包含中文標題列與樣式。
        /// </summary>
        public async Task<byte[]> ExportProductsToExcelAsync()
        {
            // 1. 查詢所有未刪除商品，並 Include Category 以讀取分類名稱
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .ToListAsync();

            // 2. 建立 ExcelPackage，會自動 dispose
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            // 3. 設定中文表頭
            string[] headers = new[]
            {
                "商品名稱",
                "商品分類名稱",
                "價格",
                "庫存量",
                "商品描述"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // 4. 為表頭設定樣式 (粗體、背景色、置中、框線)
            using (var headerRange = worksheet.Cells[1, 1, 1, headers.Length])
            {
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Size = 12;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(52, 152, 219)); // 藍色
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // 5. 逐行寫入資料
            int row = 2;
            foreach (var p in products)
            {
                worksheet.Cells[row, 1].Value = p.Name;
                worksheet.Cells[row, 2].Value = p.Category?.Name ?? string.Empty;
                worksheet.Cells[row, 3].Value = p.Price;
                worksheet.Cells[row, 4].Value = p.StockQuantity;
                worksheet.Cells[row, 5].Value = p.Description ?? string.Empty;
                row++;
            }

            // 6. 價格欄位格式化為貨幣
            if (products.Count > 0)
            {
                worksheet.Cells[2, 3, row - 1, 3].Style.Numberformat.Format = "#,##0.00";
            }

            // 7. 為資料區塊加上外框
            if (products.Count > 0)
            {
                var dataRange = worksheet.Cells[2, 1, row - 1, headers.Length];
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // 8. 自動調整欄寬
            worksheet.Cells[1, 1, row - 1, headers.Length].AutoFitColumns();

            // 9. 設定工作表預設欄寬下限
            worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 20);
            worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 18);
            worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 30);

            // 10. 凍結首列
            worksheet.View.FreezePanes(2, 1);

            // 11. 輸出 byte[]
            return await Task.FromResult(package.GetAsByteArray());
        }

        /// <summary>
        /// 從使用者上傳的 Excel 檔案批次匯入商品。
        /// 流程：
        ///   1. 讀取 .xlsx
        ///   2. 逐列解析 (略過標題列)
        ///   3. 依「商品分類名稱」查找或自動建立 ProductCategory
        ///   4. 批次寫入 Products (產生 SnowflakeId)
        /// </summary>
        public async Task<ApiResult> ImportProductsFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResult { Success = false, Message = "請上傳有效的 Excel 檔案" };
            }

            // 1. 檢查副檔名
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                return new ApiResult { Success = false, Message = "僅支援 .xlsx 格式的 Excel 檔案" };
            }

            var successList = new List<string>();
            var failedList = new List<string>();
            int totalRows = 0;

            // 2. 預先載入所有分類，轉成「不區分大小寫」的字典，避免重複查詢資料庫
            var categoryDict = await _context.ProductCategories
                .ToDictionaryAsync(c => c.Name.Trim().ToLower(), c => c.Id);

            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);

            if (package.Workbook.Worksheets.Count == 0)
            {
                return new ApiResult { Success = false, Message = "Excel 檔案中沒有工作表" };
            }

            var worksheet = package.Workbook.Worksheets[0];
            int rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount < 2)
            {
                return new ApiResult { Success = false, Message = "Excel 檔案中沒有資料" };
            }

            // 3. 逐列讀取 (第 1 列為標題，從第 2 列開始)
            var newProducts = new List<Product>();
            int maxDisplayOrder = categoryDict.Count > 0
                ? await _context.ProductCategories.MaxAsync(c => (int?)c.DisplayOrder) ?? 0
                : 0;

            for (int row = 2; row <= rowCount; row++)
            {
                totalRows++;
                try
                {
                    var name = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var categoryName = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var priceCell = worksheet.Cells[row, 3].Value;
                    var stockCell = worksheet.Cells[row, 4].Value;
                    var description = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                    // 4. 必填欄位驗證
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        failedList.Add($"第 {row} 列：商品名稱不可為空");
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(categoryName))
                    {
                        failedList.Add($"第 {row} 列：商品分類名稱不可為空");
                        continue;
                    }

                    // 5. 解析價格
                    if (!decimal.TryParse(priceCell?.ToString(), out decimal price) || price < 0)
                    {
                        failedList.Add($"第 {row} 列：價格格式錯誤或為負數");
                        continue;
                    }

                    // 6. 解析庫存量
                    if (!int.TryParse(stockCell?.ToString(), out int stock) || stock < 0)
                    {
                        failedList.Add($"第 {row} 列：庫存量格式錯誤或為負數");
                        continue;
                    }

                    // 7. 依分類名稱查找 ID，查不到時自動新增
                    var key = categoryName.ToLower();
                    if (!categoryDict.TryGetValue(key, out int categoryId))
                    {
                        var newCategory = new ProductCategory
                        {
                            Name = categoryName,
                            DisplayOrder = ++maxDisplayOrder
                        };
                        _context.ProductCategories.Add(newCategory);
                        await _context.SaveChangesAsync(); // 立即 Save 以取得新 ID
                        categoryId = newCategory.Id;
                        categoryDict[key] = categoryId;
                    }

                    // 8. 建立 Product 實體
                    var product = new Product
                    {
                        SnowflakeId = _idGenerator.CreateId(),
                        Name = name,
                        Description = string.IsNullOrWhiteSpace(description) ? null : description,
                        Price = price,
                        StockQuantity = stock,
                        CategoryId = categoryId,
                        IsAvailable = true,
                        Photo = "default-product.jpg",
                        IsDeleted = false
                    };
                    newProducts.Add(product);
                    successList.Add(name);
                }
                catch (Exception ex)
                {
                    failedList.Add($"第 {row} 列：解析失敗 - {ex.Message}");
                }
            }

            // 9. 批次寫入 Products (一次性 SaveChanges)
            if (newProducts.Count > 0)
            {
                _context.Products.AddRange(newProducts);
                await _context.SaveChangesAsync();
            }

            // 10. 若匯入過程中建立了新分類，主動清除分類快取以確保一致性
            _cache.Remove(CacheKeys.Categories);

            return new ApiResult
            {
                Success = true,
                Message = $"匯入完成：成功 {successList.Count} 筆、失敗 {failedList.Count} 筆、總共 {totalRows} 筆"
            };
        }



    }
}
