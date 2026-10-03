using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(200)]
        [RegularExpression(@"^[A-Za-z0-9_.-]+@[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}$",
        ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$",
        ErrorMessage = "Password needs to be at least 6 characters long, and needs to contain: an uppercase letter, a lowercase letter, a number and a special character.")]
        public string Password { get; set; } = string.Empty;
    }
}
