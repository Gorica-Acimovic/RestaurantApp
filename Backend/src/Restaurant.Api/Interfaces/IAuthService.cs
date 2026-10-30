using Restaurant.Api.DTOs;

namespace Restaurant.Api.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<(LoginResponse? Response, string? Error, int StatusCode)> PinLoginAsync(PinLoginRequest pinLoginRequest, string clientKey);
        Task <LoginResponse?> RegisterAsync(RegisterRequest request);
        Task<LoginResponse?> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);

    }
}
