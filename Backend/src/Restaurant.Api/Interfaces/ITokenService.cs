using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) CreateAccessToken(User user);
        string CreateRefreshToken();
    }
}
