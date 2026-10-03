using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class RefreshRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
