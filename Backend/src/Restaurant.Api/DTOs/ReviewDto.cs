namespace Restaurant.Api.DTOs
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ManagerResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? ResponderName { get; set; }
        public string RowVersion { get; set; } = string.Empty;
    }
}
