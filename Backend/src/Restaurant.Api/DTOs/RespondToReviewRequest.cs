using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class RespondToReviewRequest
    {
        [Required(ErrorMessage = "Response is required.")]
        [StringLength(1000, ErrorMessage = "Response can have at most 1000 characters.")]
        public string Response { get; set; } = string.Empty;

        [Required]
        public string RowVersion { get; set; } = string.Empty;
    }
}
