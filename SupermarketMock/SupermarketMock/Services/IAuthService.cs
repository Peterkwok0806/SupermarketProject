using SupermarketMock.DTOs;

namespace SupermarketMock.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(UserRegisterDto request);
    }
}
