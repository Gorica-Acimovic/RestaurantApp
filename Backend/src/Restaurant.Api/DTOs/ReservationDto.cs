namespace Restaurant.Api.DTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int NumberOfGuests { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? TableId { get; set; }
        public int? TableNumber { get; set; }
        public string RowVersion { get; set; } = string.Empty;
        public bool HasReview { get; set; }
    }
}
