using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace SupermarketMock.Services
{
    public class AuthService : IAuthService
    {
        private readonly SupermarketContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(SupermarketContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

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

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.email);

            if (user == null)
                return new AuthResult { success = false, message = "Email 或密碼錯誤" };

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.password, user.PasswordHash);

            if (!isPasswordValid)
                return new AuthResult { success = false, message = "Email 或密碼錯誤" };

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            var userdto = new UserDto
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email
            };

            return new AuthResult
            {
                success = true,
                message = "登入成功",
                token=token,
                userdto = userdto
            };

        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiresInMinutes),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
            

         
            






   
}
