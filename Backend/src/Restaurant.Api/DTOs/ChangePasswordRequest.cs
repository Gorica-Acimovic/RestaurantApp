using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$",
            ErrorMessage = "Password needs at least 6 characters: an uppercase letter, a lowercase letter, a number and a special character.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
