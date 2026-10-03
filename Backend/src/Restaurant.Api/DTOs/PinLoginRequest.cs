using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class PinLoginRequest
    {
        [Required]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Pin must be a 4-digit number.")]
        public string Pin { get; set; } = string.Empty;

    }
}
