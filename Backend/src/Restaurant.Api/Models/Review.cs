namespace Restaurant.Api.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;

        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ManagerResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
        public int? ResponderId { get; set; }
        public User? Responder { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
