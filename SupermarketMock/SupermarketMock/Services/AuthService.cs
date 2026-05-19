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
                email = user.Email,
                role= user.Role
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
                email = user.Email,
                role = user.Role
            };

            return new AuthResult
            {
                success = true,
                message = "登入成功",
                token=token,
                userdto = userdto
            };

        }

        public async Task<AuthResult> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new AuthResult { success = false, message = "使用者不存在" };

            if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == dto.Username && u.Id != userId))
                    return new AuthResult { success = false, message = "此 Username 已被使用" };
                user.Username = dto.Username;
            }

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
                    return new AuthResult { success = false, message = "此 Email 已被使用" };

                user.Email = dto.Email;
            }

            await _context.SaveChangesAsync();

            var newToken = GenerateJwtToken(user);

            var userdto = new UserDto
            {
                userId = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role
            };

            return new AuthResult
            {
                success = true,
                message = "個人資料更新成功",
                token = newToken,
                userdto = userdto
            };

        }

        public async Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return new AuthResult { success = false, message = "使用者不存在" };

            // 驗證目前密碼
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                return new AuthResult { success = false, message = "目前密碼錯誤" };

            // 更新密碼
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();

            return new AuthResult
            {
               success = true,
               message = "密碼更新成功"
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
