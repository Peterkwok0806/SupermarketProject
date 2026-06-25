using SupermarketMock.DTOs;

namespace SupermarketMock.IServices
{
    public interface IAdminService
    {
        Task<PagedResultDto<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search = null);
        Task<ApiResult> UpdateUserStatusAsync(int userId, bool isActive);
        Task<ApiResult> UpdateUserRoleAsync(int userId, string role);
    }
}