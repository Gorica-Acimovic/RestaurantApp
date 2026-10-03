using Microsoft.IdentityModel.Tokens;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Restaurant.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
       

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public (string Token, DateTime ExpiresAt) CreateAccessToken(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public string CreateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
    }
}
