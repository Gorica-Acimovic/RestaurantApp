namespace Restaurant.Api.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public int NumberOfGuests { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public int? UserId { get; set; } 
        public User? User { get; set; }

        public int? TableId { get; set; }
        public RestaurantTable? Table { get; set; }
        public Review? Review { get; set; }
    }
}
