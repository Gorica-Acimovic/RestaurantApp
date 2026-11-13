using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Xml.Linq;
using System.ComponentModel;
using System;
using Microsoft.ML.Runtime;
using System.Runtime.ConstrainedExecution;

namespace Restaurant.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokens;
        private readonly IConfiguration _config;
        private readonly IPinHasher _pinHasher;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext db, ITokenService tokens, IConfiguration config, IPinHasher pinHasher, TimeProvider timeProvider,ILogger<AuthService> logger )
        {
            _db = db;
            _tokens = tokens;
            _config = config;   
            _pinHasher = pinHasher;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null
                || !user.IsActive
                || StaffRoles.WithPin.Contains(user.Role)
                || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Failed login for {Email}", request.Email);
                return null;
            }

            _logger.LogInformation("User {UserId} ({Role}) logged in", user.Id, user.Role);
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
                _logger.LogWarning("Registration rejected, email {Email} already in use", request.Email);
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
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return null;
            }
            _logger.LogInformation("Guest {UserId} registered", user.Id);
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


        public async Task<(LoginResponse? Response, string? Error, int StatusCode)> PinLoginAsync(PinLoginRequest pinLoginRequest, string clientKey)
        {
            var maxFailed = int.Parse(_config["PinProtection:MaxFailedAttempts"]!);
            var blockMinutes = int.Parse(_config["PinProtection:BlockMinutes"]!);

            await EnsureAttemptRowAsync(clientKey);

            await using var transaction = await _db.Database.BeginTransactionAsync();       
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM PinLoginAttempts WITH (UPDLOCK, ROWLOCK) WHERE ClientKey = {clientKey}"
                );

            var attempt = await _db.PinLoginAttempts.FirstAsync( a => a.ClientKey == clientKey );
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            if(attempt.BlockedUntil > now)
            {
                var minutesLeft = (int)Math.Ceiling((attempt.BlockedUntil.Value - now).TotalMinutes);

                _logger.LogWarning("PIN login rejected for {ClientKey}, still blocked", clientKey);

                return (null, $"Too many failed attempts. Try again in {minutesLeft} minute(s).", 429);

            }

            var pinHash = _pinHasher.HashPin(pinLoginRequest.Pin);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.PinHash == pinHash && u.IsActive && StaffRoles.WithPin.Contains(u.Role));

            attempt.LastAttemptAt = now;
            attempt.BlockedUntil = null;

            if(user is null)
            {
                attempt.FailedCount++;

                var blocked = attempt.FailedCount >= maxFailed;

                if(blocked)
                {
                    attempt.BlockedUntil = now.AddMinutes(blockMinutes);
                    attempt.FailedCount = 0;
                }
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                if (blocked)
                {
                    _logger.LogWarning("PIN login blocked for {ClientKey} for {BlockMinutes} minute(s)", clientKey, blockMinutes);
                }
                else
                {
                    _logger.LogWarning("Invalid PIN from {ClientKey}", clientKey);
                }

                var message = blocked ? $"Invalid pin. Login is blocked for {blockMinutes} minute(s)." : "Invalid pin.";

                return (null, message, 401);
            }

            attempt.FailedCount = 0;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("User {UserId} ({Role}) logged in with PIN from {ClientKey}", user.Id, user.Role, clientKey);
            return (await CreateLoginResponseAsync(user), null, 200);

        }

        private async Task EnsureAttemptRowAsync(string clientKey)
        {
            var exists = await _db.PinLoginAttempts.AnyAsync(a => a.ClientKey == clientKey); 
            if (exists)
            {
                return;
            }

            _db.PinLoginAttempts.Add(new PinLoginAttempt                                
            {
                ClientKey = clientKey,
                LastAttemptAt = _timeProvider.GetUtcNow().UtcDateTime
            });

            try                                                                                                                
            {
                await _db.SaveChangesAsync();                                                                                       
            }
            catch(DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627})
            {
                _db.ChangeTracker.Clear();
            }
        }
    }
}
