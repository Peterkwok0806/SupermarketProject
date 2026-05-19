using SupermarketMock.DTOs;
using SupermarketMock.Models;

namespace SupermarketMock.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegisterDto request);
        Task<AuthResult> LoginAsync(LoginDto dto);

        Task<AuthResult> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<AuthResult> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }

    public class AuthResult
    {
        public bool success { get; set; }
        public string message { get; set; } = string.Empty;
        public string? token { get; set; } 
        public UserDto? userdto { get; set; }
    }


}
