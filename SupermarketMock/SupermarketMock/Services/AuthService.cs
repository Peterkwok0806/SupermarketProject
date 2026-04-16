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

        public async Task<string> RegisterAsync(UserRegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new Exception("該 Email 已被註冊");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash
                // Role 和 CreatedAt 在 Model 裡已有預設值，這裡可省
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return "註冊成功！";
        }
    }
}
