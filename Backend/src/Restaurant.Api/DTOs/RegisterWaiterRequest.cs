using Restaurant.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class RegisterWaiterRequest
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

        [Required(ErrorMessage = "Pin is required.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Pin must be a 4-digit number.")]
        public string Pin { get; set; } = string.Empty;

        public Role Role { get; set; } = Role.Waiter;
    }
}
