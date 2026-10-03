using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IConfiguration _config;
        private readonly IPinHasher _pinHasher;

        public AuthService(AppDbContext db, ITokenService tokens, IConfiguration config, IPinHasher pinHasher)
        {
            _db = db;
            _tokens = tokens;
            _config = config;   
            _pinHasher = pinHasher;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null
                || !user.IsActive
                || user.Role == Role.Waiter
                || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            return await CreateLoginResponseAsync(user);
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (stored is null
                || stored.IsRevoked
                || stored.ExpiresAt < DateTime.UtcNow
                || !stored.User.IsActive)
            {
                return null;
            }

            var (accessToken, expiresAt) = _tokens.CreateAccessToken(stored.User);

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = stored.Token,
                ExpiresAt = expiresAt,
                FirstName = stored.User.FirstName,
                LastName = stored.User.LastName,
                Role = stored.User.Role.ToString()
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
            if (stored is not null)
            {
                stored.IsRevoked = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
        {
            var alreadyExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (alreadyExists)
            {
                return null;
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = Role.Guest,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return await CreateLoginResponseAsync(user);
        }



        private async Task<LoginResponse> CreateLoginResponseAsync(User user)
        {
            var (accessToken, expiresAt) = _tokens.CreateAccessToken(user);

            var refreshToken = new RefreshToken
            {
                Token = _tokens.CreateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"]!)),
                IsRevoked = false,
                UserId = user.Id
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = expiresAt,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            };
        }

        public async Task<LoginResponse?> PinLoginAsync(PinLoginRequest pinLoginRequest)
        {
            var pinHash = _pinHasher.HashPin(pinLoginRequest.Pin);
            var user = _db.Users.FirstOrDefault(u => u.PinHash == pinHash && u.IsActive && u.Role == Role.Waiter);

            if (user is null)
            {
                return null;
            }

            return await CreateLoginResponseAsync(user);
        }
    }
}
