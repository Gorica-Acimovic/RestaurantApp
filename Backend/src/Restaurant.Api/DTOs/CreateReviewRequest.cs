using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class CreateReviewRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Reservation is required.")]
        public int ReservationId { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment can have at most 1000 characters.")]
        public string? Comment { get; set; }
    }

}
