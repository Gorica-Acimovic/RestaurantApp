namespace Restaurant.Api.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
