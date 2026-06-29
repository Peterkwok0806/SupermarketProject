using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public class CouponService : ICouponService
    {
        private readonly SupermarketContext _context;

        public CouponService(SupermarketContext context)
        {
            _context = context;
        }

        // ==================== Admin CRUD ====================

        public async Task<ApiResult<CouponListDto>> CreateCouponAsync(CreateCouponDto dto, int adminUserId)
        {
            // Validate code uniqueness
            if (await _context.Coupons.AnyAsync(c => c.Code == dto.Code))
            {
                return Fail<CouponListDto>("Coupon code already exists");
            }

            if (dto.Type == CouponType.Percentage && (dto.DiscountValue <= 0 || dto.DiscountValue > 100))
            {
                return Fail<CouponListDto>("Percent discount must be between 0 and 100");
            }

            if (dto.Type == CouponType.FixedAmount && dto.DiscountValue <= 0)
            {
                return Fail<CouponListDto>("Fixed discount must be greater than 0");
            }

            var coupon = new Coupon
            {
                Code = dto.Code.ToUpper().Trim(),
                Description = dto.Description,
                Type = dto.Type,
                DiscountValue = dto.DiscountValue,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                UsageLimit = dto.UsageLimit,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                Scope = dto.Scope,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                UsedCount = 0,
                CreatedByUserId = adminUserId,
                CreatedAt = DateTime.Now
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            // Add product/category associations
            if (dto.Scope == CouponScope.Product && dto.ProductIds?.Any() == true)
            {
                foreach (var pid in dto.ProductIds)
                {
                    _context.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = pid });
                }
                await _context.SaveChangesAsync();
            }
            else if (dto.Scope == CouponScope.Category && dto.CategoryIds?.Any() == true)
            {
                foreach (var cid in dto.CategoryIds)
                {
                    _context.CouponCategories.Add(new CouponCategory { CouponId = coupon.Id, CategoryId = cid });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(MapToCouponListDto(coupon, dto.ProductIds, dto.CategoryIds));
        }

        public async Task<ApiResult<CouponListDto>> UpdateCouponAsync(UpdateCouponDto dto)
        {
            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                .Include(c => c.CouponCategories)
                .FirstOrDefaultAsync(c => c.Id == dto.Id);

            if (coupon == null)
                return Fail<CouponListDto>("Coupon not found");

            // Check code uniqueness (excluding self)
            if (await _context.Coupons.AnyAsync(c => c.Code == dto.Code && c.Id != dto.Id))
                return Fail<CouponListDto>("Coupon code already exists");

            coupon.Code = dto.Code.ToUpper().Trim();
            coupon.Description = dto.Description;
            coupon.Type = dto.Type;
            coupon.DiscountValue = dto.DiscountValue;
            coupon.MinimumOrderAmount = dto.MinimumOrderAmount;
            coupon.MaximumDiscountAmount = dto.MaximumDiscountAmount;
            coupon.UsageLimit = dto.UsageLimit;
            coupon.UsageLimitPerUser = dto.UsageLimitPerUser;
            coupon.Scope = dto.Scope;
            coupon.StartDate = dto.StartDate;
            coupon.EndDate = dto.EndDate;
            coupon.IsActive = dto.IsActive;
            coupon.UpdatedAt = DateTime.Now;

            // Update product/category associations
            _context.CouponProducts.RemoveRange(coupon.CouponProducts);
            _context.CouponCategories.RemoveRange(coupon.CouponCategories);

            if (dto.Scope == CouponScope.Product && dto.ProductIds?.Any() == true)
            {
                foreach (var pid in dto.ProductIds)
                {
                    _context.CouponProducts.Add(new CouponProduct { CouponId = coupon.Id, ProductId = pid });
                }
            }
            else if (dto.Scope == CouponScope.Category && dto.CategoryIds?.Any() == true)
            {
                foreach (var cid in dto.CategoryIds)
                {
                    _context.CouponCategories.Add(new CouponCategory { CouponId = coupon.Id, CategoryId = cid });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(MapToCouponListDto(coupon, dto.ProductIds, dto.CategoryIds));
        }

        public async Task<ApiResult> DeleteCouponAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
                return Fail("Coupon not found");

            // Check if coupon has been used in any orders
            var hasUsages = await _context.CouponUsages.AnyAsync(u => u.CouponId == couponId);
            if (hasUsages)
                return Fail("Cannot delete coupon that has been used in orders. Deactivate it instead.");

            // Remove related associations first
            var products = await _context.CouponProducts.Where(cp => cp.CouponId == couponId).ToListAsync();
            var categories = await _context.CouponCategories.Where(cc => cc.CouponId == couponId).ToListAsync();
            _context.CouponProducts.RemoveRange(products);
            _context.CouponCategories.RemoveRange(categories);

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync();
            return Ok("Coupon deleted");
        }

        public async Task<ApiResult<CouponListDto>> GetCouponByIdAsync(int couponId)
        {
            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                .Include(c => c.CouponCategories)
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return Fail<CouponListDto>("Coupon not found");

            var productIds = coupon.CouponProducts.Select(cp => cp.ProductId).ToList();
            var categoryIds = coupon.CouponCategories.Select(cc => cc.CategoryId).ToList();

            return Ok(MapToCouponListDto(coupon, productIds, categoryIds));
        }

        public async Task<ApiResultPagination<CouponListDto>> GetCouponsAsync(
            string? search, CouponType? type, bool? isActive, bool? isExpired,
            string? sort, int page, int pageSize)
        {
            var query = _context.Coupons
                .Include(c => c.CouponProducts)
                .Include(c => c.CouponCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Code.Contains(search) || (c.Description != null && c.Description.Contains(search)));

            if (type.HasValue)
                query = query.Where(c => c.Type == type.Value);

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (isExpired.HasValue)
            {
                var now = DateTime.Now;
                if (isExpired.Value)
                    query = query.Where(c => c.EndDate < now);
                else
                    query = query.Where(c => c.EndDate >= now);
            }

            query = sort switch
            {
                "code" => query.OrderBy(c => c.Code),
                "-code" => query.OrderByDescending(c => c.Code),
                "created" => query.OrderBy(c => c.CreatedAt),
                "-created" => query.OrderByDescending(c => c.CreatedAt),
                "usage" => query.OrderBy(c => c.UsedCount),
                "-usage" => query.OrderByDescending(c => c.UsedCount),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CouponListDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Description = c.Description,
                    Type = c.Type,
                    DiscountValue = c.DiscountValue,
                    MinimumOrderAmount = c.MinimumOrderAmount,
                    MaximumDiscountAmount = c.MaximumDiscountAmount,
                    UsageLimit = c.UsageLimit,
                    UsedCount = c.UsedCount,
                    UsageLimitPerUser = c.UsageLimitPerUser,
                    Scope = c.Scope,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    ProductIds = c.CouponProducts.Select(cp => cp.ProductId).ToList(),
                    CategoryIds = c.CouponCategories.Select(cc => cc.CategoryId).ToList()
                })
                .ToListAsync();

            return new ApiResultPagination<CouponListDto>
            {
                Success = true,
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<CouponStatsDto> GetCouponStatsAsync()
        {
            var now = DateTime.Now;
            var all = await _context.Coupons.ToListAsync();
            return new CouponStatsDto
            {
                TotalCoupons = all.Count,
                ActiveCoupons = all.Count(c => c.IsActive && c.EndDate >= now),
                ExpiredCoupons = all.Count(c => c.EndDate < now),
                TotalRedemptions = all.Sum(c => c.UsedCount),
                TotalDiscountGiven = await _context.CouponUsages.SumAsync(u => u.DiscountApplied)
            };
        }

        public async Task<ApiResult<bool>> ToggleCouponActiveAsync(int couponId)
        {
            var coupon = await _context.Coupons.FindAsync(couponId);
            if (coupon == null)
                return Fail<bool>("Coupon not found");

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(coupon.IsActive);
        }

        // ==================== Customer Actions ====================

        public async Task<ApiResultPagination<CouponListDto>> GetAvailableCouponsAsync()
        {
            var now = DateTime.Now;
            var coupons = await _context.Coupons
                .Include(c => c.CouponProducts)
                .Include(c => c.CouponCategories)
                .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CouponListDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Description = c.Description,
                    Type = c.Type,
                    DiscountValue = c.DiscountValue,
                    MinimumOrderAmount = c.MinimumOrderAmount,
                    MaximumDiscountAmount = c.MaximumDiscountAmount,
                    UsageLimit = c.UsageLimit,
                    UsedCount = c.UsedCount,
                    UsageLimitPerUser = c.UsageLimitPerUser,
                    Scope = c.Scope,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    ProductIds = c.CouponProducts.Select(cp => cp.ProductId).ToList(),
                    CategoryIds = c.CouponCategories.Select(cc => cc.CategoryId).ToList()
                })
                .ToListAsync();

            return new ApiResultPagination<CouponListDto>
            {
                Success = true,
                Items = coupons,
                TotalCount = coupons.Count,
                PageNumber = 1,
                PageSize = coupons.Count
            };
        }

        public async Task<ApiResult<CouponValidationResultDto>> ValidateCouponAsync(ValidateCouponRequestDto dto, int userId)
        {
            var coupon = await _context.Coupons
                .Include(c => c.CouponProducts)
                .Include(c => c.CouponCategories)
                .FirstOrDefaultAsync(c => c.Code == dto.Code.ToUpper().Trim());

            if (coupon == null)
                return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Invalid coupon code" });

            if (!coupon.IsActive)
                return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon is no longer active" });

            var now = DateTime.Now;
            if (coupon.StartDate > now)
                return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon has not started yet" });

            if (coupon.EndDate < now)
                return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon has expired" });

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon usage limit reached" });

            if (coupon.UsageLimitPerUser.HasValue)
            {
                var userUsageCount = await _context.CouponUsages
                    .CountAsync(u => u.CouponId == coupon.Id && u.UserId == userId);
                if (userUsageCount >= coupon.UsageLimitPerUser.Value)
                    return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "You have reached the usage limit for this coupon" });
            }

            if (coupon.MinimumOrderAmount.HasValue && dto.OrderSubtotal < coupon.MinimumOrderAmount.Value)
                return Ok(new CouponValidationResultDto
                {
                    IsValid = false,
                    ErrorMessage = $"Minimum order amount is {coupon.MinimumOrderAmount.Value:C}"
                });

            // Scope validation
            if (coupon.Scope == CouponScope.Product)
            {
                var couponProductIds = coupon.CouponProducts.Select(cp => cp.ProductId).ToList();
                if (dto.CartProductIds == null || !dto.CartProductIds.Any(id => couponProductIds.Contains(id)))
                    return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon is not applicable to any items in your cart" });
            }
            else if (coupon.Scope == CouponScope.Category)
            {
                var couponCategoryIds = coupon.CouponCategories.Select(cc => cc.CategoryId).ToList();
                if (dto.CartCategoryIds == null || !dto.CartCategoryIds.Any(id => couponCategoryIds.Contains(id)))
                    return Ok(new CouponValidationResultDto { IsValid = false, ErrorMessage = "Coupon is not applicable to any items in your cart" });
            }

            // Calculate discount
            decimal discount = CalculateDiscount(coupon, dto.OrderSubtotal);

            return Ok(new CouponValidationResultDto
            {
                IsValid = true,
                CouponId = coupon.Id,
                Code = coupon.Code,
                Type = coupon.Type,
                DiscountAmount = discount,
                Description = coupon.Description
            });
        }

        public async Task<ApiResult<bool>> ApplyCouponToOrderAsync(string code, int orderId, int userId)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code.ToUpper().Trim());
            if (coupon == null)
                return Fail<bool>("Coupon not found");

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
            if (order == null)
                return Fail<bool>("Order not found");

            if (order.CouponId.HasValue)
                return Fail<bool>("Coupon already applied to this order");

            // Validate again
            var validateResult = await ValidateCouponAsync(new ValidateCouponRequestDto
            {
                Code = code,
                OrderSubtotal = order.TotalAmount
            }, userId);

            if (!validateResult.Success || validateResult.Item?.IsValid != true)
                return Fail<bool>(validateResult.Item?.ErrorMessage ?? "Coupon is not valid");

            // Apply discount to order
            var discount = validateResult.Item.DiscountAmount;
            order.CouponId = coupon.Id;
            order.DiscountAmount = discount;
            order.TotalAmount = Math.Max(0, order.TotalAmount - discount);
            order.UpdatedAt = DateTime.Now;

            // Record usage
            _context.CouponUsages.Add(new CouponUsage
            {
                CouponId = coupon.Id,
                UserId = userId,
                OrderId = orderId,
                DiscountApplied = discount,
                UsedAt = DateTime.Now
            });

            // Increment used count
            coupon.UsedCount++;
            coupon.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(true);
        }

        public async Task<ApiResultPagination<CouponUsageDto>> GetUserCouponHistoryAsync(int userId, int page, int pageSize)
        {
            var query = _context.CouponUsages
                .Include(u => u.Coupon)
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.UsedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new CouponUsageDto
                {
                    Id = u.Id,
                    CouponCode = u.Coupon.Code,
                    CouponDescription = u.Coupon.Description,
                    CouponType = u.Coupon.Type,
                    DiscountApplied = u.DiscountApplied,
                    UsedAt = u.UsedAt,
                    OrderId = u.OrderId
                })
                .ToListAsync();

            return new ApiResultPagination<CouponUsageDto>
            {
                Success = true,
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        // ==================== Helpers ====================

        private static decimal CalculateDiscount(Coupon coupon, decimal orderSubtotal)
        {
            decimal discount = coupon.Type switch
            {
                CouponType.Percentage => orderSubtotal * (coupon.DiscountValue / 100m),
                CouponType.FixedAmount => coupon.DiscountValue,
                CouponType.FreeShipping => 0, // shipping discount handled separately
                _ => 0
            };

            if (coupon.MaximumDiscountAmount.HasValue && discount > coupon.MaximumDiscountAmount.Value)
                discount = coupon.MaximumDiscountAmount.Value;

            discount = Math.Min(discount, orderSubtotal);
            return Math.Round(discount, 2);
        }

        private static CouponListDto MapToCouponListDto(Coupon coupon, List<int>? productIds = null, List<int>? categoryIds = null)
        {
            return new CouponListDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Description = coupon.Description,
                Type = coupon.Type,
                DiscountValue = coupon.DiscountValue,
                MinimumOrderAmount = coupon.MinimumOrderAmount,
                MaximumDiscountAmount = coupon.MaximumDiscountAmount,
                UsageLimit = coupon.UsageLimit,
                UsedCount = coupon.UsedCount,
                UsageLimitPerUser = coupon.UsageLimitPerUser,
                Scope = coupon.Scope,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                IsActive = coupon.IsActive,
                CreatedAt = coupon.CreatedAt,
                ProductIds = productIds ?? coupon.CouponProducts?.Select(cp => cp.ProductId).ToList(),
                CategoryIds = categoryIds ?? coupon.CouponCategories?.Select(cc => cc.CategoryId).ToList()
            };
        }

        private static ApiResult Ok(string message) => new() { Success = true, Message = message };
        private static ApiResult<T> Ok<T>(T item) => new() { Success = true, Item = item };
        private static ApiResult Fail(string msg) => new() { Success = false, Message = msg };
        private static ApiResult<T> Fail<T>(string msg) => new() { Success = false, Message = msg };
    }
}