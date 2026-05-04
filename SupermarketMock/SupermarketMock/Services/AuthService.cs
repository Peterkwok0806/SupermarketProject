using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public class AuthService : IAuthService
    {
        private readonly SupermarketContext _context;

        public AuthService(SupermarketContext context) { _context = context; }

        public async Task<AuthResult> RegisterAsync(UserRegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "此 Email 已被註冊"
                };
            }

            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "此 Username 已被使用"
                };
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.Id,
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                Message = "註冊成功！",
                User = user
            };

        }






    }
}
