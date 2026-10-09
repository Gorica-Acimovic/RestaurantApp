using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class ReservationRequest
    {
        [Required, MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? CustomerEmail { get; set; }

        [MaxLength(20)]
        public string? CustomerPhone { get; set; }
        public DateTime StartsAt { get; set; }

        [Range(1,50)]
        public int NumberOfGuests { get; set; }

    }
}
