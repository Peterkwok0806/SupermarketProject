using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.IServices
{
    public interface ICouponService
    {
        // ===== Admin CRUD =====
        Task<ApiResult<CouponListDto>> CreateCouponAsync(CreateCouponDto dto, int adminUserId);
        Task<ApiResult<CouponListDto>> UpdateCouponAsync(UpdateCouponDto dto);
        Task<ApiResult> DeleteCouponAsync(int couponId);
        Task<ApiResult<CouponListDto>> GetCouponByIdAsync(int couponId);
        Task<ApiResultPagination<CouponListDto>> GetCouponsAsync(
            string? search, CouponType? type, bool? isActive, bool? isExpired,
            string? sort, int page, int pageSize);
        Task<CouponStatsDto> GetCouponStatsAsync();
        Task<ApiResult<bool>> ToggleCouponActiveAsync(int couponId);

        // ===== Customer Actions =====
        Task<ApiResultPagination<CouponListDto>> GetAvailableCouponsAsync();
        Task<ApiResult<CouponValidationResultDto>> ValidateCouponAsync(ValidateCouponRequestDto dto, int userId);
        Task<ApiResult<bool>> ApplyCouponToOrderAsync(string code, int orderId, int userId);
        Task<ApiResultPagination<CouponUsageDto>> GetUserCouponHistoryAsync(int userId, int page, int pageSize);
    }
}