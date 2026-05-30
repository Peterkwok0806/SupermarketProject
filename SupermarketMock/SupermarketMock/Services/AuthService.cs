using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SupermarketMock.DTOs;
using SupermarketMock.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hangfire;


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

            // 使用安全隨機數產生 6 位數驗證碼
            var verificationCode = GenerateSecureCode();

            // 儲存驗證紀錄至資料庫
            var verification = new EmailVerification
            {
                Email = dto.email,
                Username = dto.username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                Code = verificationCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.EmailVerifications.Add(verification);
            await _context.SaveChangesAsync();

            //交由 Hangfire 背景執行緒發送 EmailService
            BackgroundJob.Enqueue<EmailService>(service =>
            service.SendVerificationEmailAsync(dto.email, verificationCode));

            return new AuthResult
            {
                success = true,
                message = "驗證碼已發送至您的 Email，請輸入驗證碼以完成註冊",
            };
        }

        public async Task<AuthResult> VerifyAndRegisterAsync(VerifyCodeDto dto)
        {
            var verification = await _context.EmailVerifications
                                .Where(v => v.Email == dto.Email && !v.IsUsed)
                                .OrderByDescending(v => v.CreatedAt)
                                .FirstOrDefaultAsync();

            // 檢查驗證碼正確性
            if (verification == null || verification.Code != dto.Code)
            {
                return new AuthResult { success = false, message = "驗證碼錯誤或已失效" };
            }

            // 檢查驗證碼是否過期
            if (verification.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResult { success = false, message = "驗證碼已過期，請重新獲取" };
            }

            // 將該驗證碼標記為已使用，避免重複驗證
            verification.IsUsed = true;
            _context.EmailVerifications.Update(verification);

            var User = new User
            {
                Username = verification.Username,
                Email = verification.Email,
                PasswordHash = verification.PasswordHash, // 帶入當初暫存的密碼
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            var cart = new Cart
            {
                UserId = User.Id,
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            var userdto = new UserDto
            {
                UserId = User.Id,
                Username = User.Username,
                Email = User.Email,
                Role = User.Role
            };

            return new AuthResult
            {
                success = true,
                message = "驗證成功！帳號已順利註冊並開通。",
                userdto = userdto
            };

        }

        public async Task<AuthResult> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return new AuthResult { success = false, message = "Email 或密碼錯誤" };

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
                return new AuthResult { success = false, message = "Email 或密碼錯誤" };

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;
            user.LastLoginAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var userdto = new UserDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return new AuthResult
            {
                success = true,
                message = "登入成功",
                token=token,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = expiryTime,
                userdto = userdto
            };

        }

        public async Task<AuthResult> RefreshTokenAsync(string oldRefreshToken)
        {
            if (string.IsNullOrEmpty(oldRefreshToken))
                return new AuthResult { success = false, message = "無效的刷新憑證" };

            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == oldRefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResult { success = false, message = "憑證已過期或不合法，請重新登入" };

            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var expiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = expiryTime;

            await _context.SaveChangesAsync();

            var userdto = new UserDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return new AuthResult
            {
                success = true,
                message = "憑證刷新成功",
                token = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiryTime = expiryTime,
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
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
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

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateSecureCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }
    }
            

         
            






   
}
