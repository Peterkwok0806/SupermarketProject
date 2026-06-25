using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.IServices;

namespace SupermarketMock.Services
{
    public class AdminService : IAdminService
    {
        private readonly SupermarketContext _context;

        public AdminService(SupermarketContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<AdminUserDto>> GetUsersAsync(int page, int pageSize, string? search = null)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return new PagedResultDto<AdminUserDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<ApiResult> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResult { Success = false, Message = "用戶不存在" };
            }

            user.IsActive = isActive;
            await _context.SaveChangesAsync();

            return new ApiResult { Success = true, Message = isActive ? "用戶已啟用" : "用戶已停用" };
        }

        public async Task<ApiResult> UpdateUserRoleAsync(int userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return new ApiResult { Success = false, Message = "用戶不存在" };
            }

            if (role != "Admin" && role != "Customer")
            {
                return new ApiResult { Success = false, Message = "無效的角色" };
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            return new ApiResult { Success = true, Message = "用戶角色已更新" };
        }
    }
}