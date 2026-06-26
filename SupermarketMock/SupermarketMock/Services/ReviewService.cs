using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    /// <summary>
    /// 商品評論服務實作
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly SupermarketContext _context;
        private readonly ILogger<ReviewService> _logger;

        // 評論可編輯時間窗（天）
        private const int EditWindowDays = 7;
        // 每則評論最大附圖數
        private const int MaxImages = 5;
        // 評論字數上下限
        private const int MinContentLength = 5;
        private const int MaxContentLength = 2000;

        public ReviewService(SupermarketContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ============================================================
        //  顧客端 - 建立
        // ============================================================
        public async Task<ApiResult<ReviewDto>> CreateReviewAsync(int userId, CreateReviewDto dto)
        {
            // 基本驗證
            if (dto.Rating < 1 || dto.Rating > 5)
                return new ApiResult<ReviewDto> { Success = false, Message = "評分必須介於 1–5 顆星" };

            if (string.IsNullOrWhiteSpace(dto.Content) ||
                dto.Content.Length < MinContentLength ||
                dto.Content.Length > MaxContentLength)
                return new ApiResult<ReviewDto> { Success = false, Message = $"評論內容需介於 {MinContentLength}–{MaxContentLength} 字" };

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxImages)
                return new ApiResult<ReviewDto> { Success = false, Message = $"附圖最多 {MaxImages} 張" };

            // 檢查商品存在
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return new ApiResult<ReviewDto> { Success = false, Message = "商品不存在" };

            // 實購驗證
            bool isVerified = false;
            int? verifiedOrderId = null;

            if (dto.OrderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == dto.OrderId.Value && o.UserId == userId);

                if (order == null)
                    return new ApiResult<ReviewDto> { Success = false, Message = "訂單不存在或不屬於您" };

                if (order.Status != OrderStatus.Completed)
                    return new ApiResult<ReviewDto> { Success = false, Message = "僅有「已完成」狀態的訂單可評論" };

                var hasItem = order.OrderItems.Any(oi => oi.ProductId == dto.ProductId);
                if (!hasItem)
                    return new ApiResult<ReviewDto> { Success = false, Message = "此訂單未包含該商品" };

                isVerified = true;
                verifiedOrderId = order.Id;
            }

            // 唯一性檢查（同一使用者對同一商品同訂單僅能評論一次）
            var existingQuery = _context.ProductReviews
                .Where(r => r.UserId == userId
                    && r.ProductId == dto.ProductId
                    && !r.IsDeleted);

            if (verifiedOrderId.HasValue)
                existingQuery = existingQuery.Where(r => r.OrderId == verifiedOrderId);
            else
                existingQuery = existingQuery.Where(r => r.OrderId == null);

            if (await existingQuery.AnyAsync())
                return new ApiResult<ReviewDto> { Success = false, Message = "您已對此商品發表過評論" };

            var review = new ProductReview
            {
                UserId = userId,
                ProductId = dto.ProductId,
                OrderId = verifiedOrderId,
                Rating = dto.Rating,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim(),
                Content = dto.Content.Trim(),
                IsVerifiedPurchase = isVerified,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // 附圖
            if (dto.ImageUrls != null)
            {
                int sort = 0;
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Take(MaxImages))
                {
                    review.Images.Add(new ReviewImage
                    {
                        ImageUrl = url,
                        SortOrder = sort++
                    });
                }
            }

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            _logger.LogInformation("使用者 {UserId} 對商品 {ProductId} 發表評論 {ReviewId}", userId, dto.ProductId, review.Id);

            var result = await GetReviewByIdAsync(review.Id, userId);
            return new ApiResult<ReviewDto>
            {
                Success = true,
                Message = "評論已送出，待管理員審核",
                Item = result
            };
        }

        // ============================================================
        //  編輯（7 天內）
        // ============================================================
        public async Task<ApiResult<ReviewDto>> UpdateReviewAsync(int userId, int reviewId, UpdateReviewDto dto)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

            if (review == null)
                return new ApiResult<ReviewDto> { Success = false, Message = "評論不存在" };

            if (review.Status == ReviewStatus.Rejected)
                return new ApiResult<ReviewDto> { Success = false, Message = "已被拒絕的評論不可編輯" };

            if ((DateTime.UtcNow - review.CreatedAt).TotalDays > EditWindowDays)
                return new ApiResult<ReviewDto> { Success = false, Message = $"僅 {EditWindowDays} 天內可編輯評論" };

            if (dto.Rating < 1 || dto.Rating > 5)
                return new ApiResult<ReviewDto> { Success = false, Message = "評分必須介於 1–5 顆星" };

            if (string.IsNullOrWhiteSpace(dto.Content) ||
                dto.Content.Length < MinContentLength ||
                dto.Content.Length > MaxContentLength)
                return new ApiResult<ReviewDto> { Success = false, Message = $"評論內容需介於 {MinContentLength}–{MaxContentLength} 字" };

            if (dto.ImageUrls != null && dto.ImageUrls.Count > MaxImages)
                return new ApiResult<ReviewDto> { Success = false, Message = $"附圖最多 {MaxImages} 張" };

            review.Rating = dto.Rating;
            review.Title = string.IsNullOrWhiteSpace(dto.Title) ? null : dto.Title.Trim();
            review.Content = dto.Content.Trim();
            review.UpdatedAt = DateTime.UtcNow;
            // 編輯後重新進入待審核狀態
            review.Status = ReviewStatus.Pending;
            review.AdminReply = null;
            review.AdminReplyAt = null;

            // 重新設定附圖：先刪後建
            var oldImages = await _context.ReviewImages.Where(i => i.ReviewId == review.Id).ToListAsync();
            _context.ReviewImages.RemoveRange(oldImages);

            if (dto.ImageUrls != null)
            {
                int sort = 0;
                foreach (var url in dto.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Take(MaxImages))
                {
                    _context.ReviewImages.Add(new ReviewImage
                    {
                        ReviewId = review.Id,
                        ImageUrl = url,
                        SortOrder = sort++
                    });
                }
            }

            await _context.SaveChangesAsync();

            var updated = await GetReviewByIdAsync(review.Id, userId);
            return new ApiResult<ReviewDto>
            {
                Success = true,
                Message = "評論已更新，等待重新審核",
                Item = updated
            };
        }

        // ============================================================
        //  刪除（軟刪除）
        // ============================================================
        public async Task<ApiResult> DeleteReviewAsync(int userId, int reviewId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId && !r.IsDeleted);

            if (review == null)
                return new ApiResult { Success = false, Message = "評論不存在" };

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResult { Success = true, Message = "已刪除評論" };
        }

        // ============================================================
        //  商品評論列表（公開，僅 Approved）
        // ============================================================
        public async Task<ApiResultPagination<ReviewDto>> GetProductReviewsAsync(int productId, ReviewFilterDto filter, int? currentUserId = null)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 50);

            var query = _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && !r.IsDeleted && r.Status == ReviewStatus.Approved);

            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);

            if (filter.VerifiedOnly == true)
                query = query.Where(r => r.IsVerifiedPurchase);

            if (filter.HasImage == true)
                query = query.Where(r => r.Images.Any());

            // 排序
            query = (filter.SortBy ?? "newest").ToLower() switch
            {
                "helpful" => query.OrderByDescending(r => r.HelpfulCount).ThenByDescending(r => r.CreatedAt),
                _ => query.OrderByDescending(r => r.CreatedAt)
            };

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.User)
                .Include(r => r.Images)
                .ToListAsync();

            // 一次查詢點讚狀態
            HashSet<int> myHelpfulReviewIds = new();
            if (currentUserId.HasValue && items.Count > 0)
            {
                myHelpfulReviewIds = (await _context.ReviewHelpfuls
                    .Where(h => h.UserId == currentUserId.Value && items.Select(i => i.Id).Contains(h.ReviewId))
                    .Select(h => h.ReviewId)
                    .ToListAsync()).ToHashSet();
            }

            return new ApiResultPagination<ReviewDto>
            {
                Success = true,
                Items = items.Select(r => MapToDto(r, myHelpfulReviewIds.Contains(r.Id))).ToList(),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        // ============================================================
        //  評分彙總
        // ============================================================
        public async Task<ProductReviewStatsDto> GetProductReviewStatsAsync(int productId)
        {
            var allReviews = await _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && !r.IsDeleted && r.Status == ReviewStatus.Approved)
                .Select(r => new { r.Rating, r.IsVerifiedPurchase })
                .ToListAsync();

            var stats = new ProductReviewStatsDto
            {
                ProductId = productId,
                TotalCount = allReviews.Count,
                AverageRating = allReviews.Count == 0 ? 0 : Math.Round(allReviews.Average(r => (double)r.Rating), 2),
                FiveStarCount = allReviews.Count(r => r.Rating == 5),
                FourStarCount = allReviews.Count(r => r.Rating == 4),
                ThreeStarCount = allReviews.Count(r => r.Rating == 3),
                TwoStarCount = allReviews.Count(r => r.Rating == 2),
                OneStarCount = allReviews.Count(r => r.Rating == 1),
                VerifiedCount = allReviews.Count(r => r.IsVerifiedPurchase)
            };

            // WithImage 需查詢附圖關聯
            stats.WithImageCount = await _context.ReviewImages
                .Where(i => i.Review!.ProductId == productId
                    && !i.Review.IsDeleted
                    && i.Review.Status == ReviewStatus.Approved)
                .Select(i => i.ReviewId)
                .Distinct()
                .CountAsync();

            return stats;
        }

        // ============================================================
        //  我的評論
        // ============================================================
        public async Task<ApiResultPagination<MyReviewDto>> GetMyReviewsAsync(int userId, int page, int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 50);

            var query = _context.ProductReviews
                .AsNoTracking()
                .Where(r => r.UserId == userId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.Product)
                .Include(r => r.Images)
                .ToListAsync();

            return new ApiResultPagination<MyReviewDto>
            {
                Success = true,
                Items = items.Select(r => new MyReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? string.Empty,
                    ProductPhoto = r.Product?.Photo ?? string.Empty,
                    Rating = r.Rating,
                    Title = r.Title,
                    Content = r.Content,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    AdminReply = r.AdminReply,
                    ImageUrls = r.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList()
                }).ToList(),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        // ============================================================
        //  取得單則
        // ============================================================
        public async Task<ReviewDto?> GetReviewByIdAsync(int reviewId, int? currentUserId = null)
        {
            var r = await _context.ProductReviews
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Product)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == reviewId && !x.IsDeleted);

            if (r == null) return null;

            bool hasHelpful = false;
            if (currentUserId.HasValue)
            {
                hasHelpful = await _context.ReviewHelpfuls
                    .AnyAsync(h => h.UserId == currentUserId.Value && h.ReviewId == reviewId);
            }

            return MapToDto(r, hasHelpful);
        }

        // ============================================================
        //  切換點讚
        // ============================================================
        public async Task<ApiResult<bool>> ToggleHelpfulAsync(int userId, int reviewId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted && r.Status == ReviewStatus.Approved);

            if (review == null)
                return new ApiResult<bool> { Success = false, Message = "評論不存在或未通過審核" };

            // 不能為自己點讚
            if (review.UserId == userId)
                return new ApiResult<bool> { Success = false, Message = "無法為自己的評論點讚" };

            var existing = await _context.ReviewHelpfuls
                .FirstOrDefaultAsync(h => h.UserId == userId && h.ReviewId == reviewId);

            if (existing != null)
            {
                _context.ReviewHelpfuls.Remove(existing);
                review.HelpfulCount = Math.Max(0, review.HelpfulCount - 1);
                await _context.SaveChangesAsync();
                return new ApiResult<bool> { Success = true, Message = "已取消點讚", Item = false };
            }
            else
            {
                _context.ReviewHelpfuls.Add(new ReviewHelpful
                {
                    UserId = userId,
                    ReviewId = reviewId
                });
                review.HelpfulCount += 1;
                await _context.SaveChangesAsync();
                return new ApiResult<bool> { Success = true, Message = "已點讚", Item = true };
            }
        }

        // ============================================================
        //  檢查使用者是否可對商品評論
        // ============================================================
        public async Task<ApiResult<bool>> CanReviewProductAsync(int userId, int productId, int? orderId = null)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return new ApiResult<bool> { Success = false, Message = "商品不存在", Item = false };

            // 若帶入 OrderId：檢查是否為該使用者已完成的訂單並包含此商品
            if (orderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value && o.UserId == userId);

                if (order == null)
                    return new ApiResult<bool> { Success = false, Message = "訂單不存在或不屬於您", Item = false };

                if (order.Status != OrderStatus.Completed)
                    return new ApiResult<bool> { Success = false, Message = "僅有已完成的訂單可評論", Item = false };

                if (!order.OrderItems.Any(oi => oi.ProductId == productId))
                    return new ApiResult<bool> { Success = false, Message = "此訂單未包含該商品", Item = false };

                var alreadyReviewed = await _context.ProductReviews
                    .AnyAsync(r => r.UserId == userId && r.ProductId == productId && r.OrderId == orderId && !r.IsDeleted);
                if (alreadyReviewed)
                    return new ApiResult<bool> { Success = false, Message = "此訂單已對該商品評論過", Item = false };

                return new ApiResult<bool> { Success = true, Message = "可評論", Item = true };
            }

            // 沒帶 OrderId：檢查是否有任何已完成且含此商品的訂單
            var eligibleOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Completed)
                .Where(o => o.OrderItems.Any(oi => oi.ProductId == productId))
                .FirstOrDefaultAsync();

            if (eligibleOrder == null)
                return new ApiResult<bool> { Success = true, Message = "您目前未實購此商品（可發表非實購評論）", Item = true };

            // 檢查是否已對此商品評論過（任何訂單）
            var anyReviewed = await _context.ProductReviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted);
            if (anyReviewed)
                return new ApiResult<bool> { Success = false, Message = "您已對此商品發表過評論", Item = false };

            return new ApiResult<bool> { Success = true, Message = "可評論", Item = true };
        }

        // ============================================================
        //  後台 - 列表
        // ============================================================
        public async Task<ApiResultPagination<ReviewDto>> AdminGetReviewsAsync(AdminReviewFilterDto filter)
        {
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 20 : Math.Min(filter.PageSize, 100);

            var query = _context.ProductReviews
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);
            if (filter.ProductId.HasValue)
                query = query.Where(r => r.ProductId == filter.ProductId.Value);
            if (filter.Rating.HasValue)
                query = query.Where(r => r.Rating == filter.Rating.Value);
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(r =>
                    r.Content.Contains(kw) ||
                    (r.Title != null && r.Title.Contains(kw)) ||
                    r.User.Username.Contains(kw) ||
                    r.Product.Name.Contains(kw));
            }
            if (filter.FromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.ToDate.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Images)
                .ToListAsync();

            return new ApiResultPagination<ReviewDto>
            {
                Success = true,
                Items = items.Select(r => MapToDto(r, false)).ToList(),
                TotalCount = total,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        // ============================================================
        //  後台 - 變更狀態
        // ============================================================
        public async Task<ApiResult<ReviewDto>> AdminUpdateStatusAsync(int reviewId, ReviewStatus status, int adminUserId)
        {
            var review = await _context.ProductReviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

            if (review == null)
                return new ApiResult<ReviewDto> { Success = false, Message = "評論不存在" };

            review.Status = status;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("管理員 {AdminId} 將評論 {ReviewId} 狀態改為 {Status}", adminUserId, reviewId, status);

            return new ApiResult<ReviewDto>
            {
                Success = true,
                Message = "狀態已更新",
                Item = MapToDto(review, false)
            };
        }

        // ============================================================
        //  後台 - 官方回覆
        // ============================================================
        public async Task<ApiResult<ReviewDto>> AdminReplyAsync(int reviewId, string reply, int adminUserId)
        {
            if (string.IsNullOrWhiteSpace(reply) || reply.Length < 2)
                return new ApiResult<ReviewDto> { Success = false, Message = "回覆內容不可少於 2 字" };

            var review = await _context.ProductReviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.Images)
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);

            if (review == null)
                return new ApiResult<ReviewDto> { Success = false, Message = "評論不存在" };

            review.AdminReply = reply.Trim();
            review.AdminReplyAt = DateTime.UtcNow;
            review.AdminReplyUserId = adminUserId;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("管理員 {AdminId} 對評論 {ReviewId} 進行回覆", adminUserId, reviewId);

            return new ApiResult<ReviewDto>
            {
                Success = true,
                Message = "回覆成功",
                Item = MapToDto(review, false)
            };
        }

        // ============================================================
        //  後台 - 刪除
        // ============================================================
        public async Task<ApiResult> AdminDeleteAsync(int reviewId)
        {
            var review = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && !r.IsDeleted);
            if (review == null)
                return new ApiResult { Success = false, Message = "評論不存在" };

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new ApiResult { Success = true, Message = "已刪除評論" };
        }

        // ============================================================
        //  後台儀表板
        // ============================================================
        public async Task<ReviewDashboardDto> AdminGetDashboardAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var data = await _context.ProductReviews
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Select(r => new { r.Status, r.CreatedAt, r.Rating })
                .ToListAsync();

            return new ReviewDashboardDto
            {
                PendingCount = data.Count(r => r.Status == ReviewStatus.Pending),
                ApprovedCount = data.Count(r => r.Status == ReviewStatus.Approved),
                RejectedCount = data.Count(r => r.Status == ReviewStatus.Rejected),
                HiddenCount = data.Count(r => r.Status == ReviewStatus.Hidden),
                TodayCount = data.Count(r => r.CreatedAt >= today && r.CreatedAt < tomorrow),
                AverageRating = data.Where(r => r.Status == ReviewStatus.Approved).Any()
                    ? Math.Round(data.Where(r => r.Status == ReviewStatus.Approved).Average(r => (double)r.Rating), 2)
                    : 0
            };
        }

        // ============================================================
        //  私有方法
        // ============================================================
        private static ReviewDto MapToDto(ProductReview r, bool hasHelpful)
        {
            return new ReviewDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? string.Empty,
                UserId = r.UserId,
                UserName = r.User?.Username ?? "匿名",
                Rating = r.Rating,
                Title = r.Title,
                Content = r.Content,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                Status = r.Status,
                HelpfulCount = r.HelpfulCount,
                AdminReply = r.AdminReply,
                AdminReplyAt = r.AdminReplyAt,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ImageUrls = r.Images?.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList() ?? new List<string>(),
                HasHelpful = hasHelpful
            };
        }
    }
}