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
            if (await _context.Users.AnyAsync(u => u.Email == dto.email))
            {
                return new AuthResult
                {
                    success = false,
                    message = "此 Email 已被註冊"
                };
            }

            if (await _context.Users.AnyAsync(u => u.Username == dto.username))
            {
                return new AuthResult
                {
                    success = false,
                    message = "此 Username 已被使用"
                };
            }

            var user = new User
            {
                Username = dto.username,
                Email = dto.email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = user.Id,
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var userdto = new UserDto
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email
            };

            return new AuthResult
            {
                success = true,
                message = "註冊成功！",
                userdto = userdto
            };

        }






    }
}
