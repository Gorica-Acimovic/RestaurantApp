namespace Restaurant.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PinHash { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; } = true;

        public int? RegisteredById { get; set; }
        public User? RegisteredBy { get; set; }
    }
}
