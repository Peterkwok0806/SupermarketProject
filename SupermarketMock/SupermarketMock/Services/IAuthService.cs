using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegisterDto request);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public User? User { get; set; }
    }


}
