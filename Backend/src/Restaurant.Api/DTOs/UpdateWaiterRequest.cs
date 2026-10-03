using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class UpdateWaiterRequest
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
            ErrorMessage = "Email format is not valid.")]
        public string Email { get; set; } = string.Empty;

        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN must have exactly 4 digits.")]
        public string? Pin { get; set; }
    }
}
