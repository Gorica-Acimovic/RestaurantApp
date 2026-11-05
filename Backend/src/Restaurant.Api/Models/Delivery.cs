namespace Restaurant.Api.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
      
        public string? Address { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }

        public decimal DeliveryFee { get; set; }
        public PaymentMethod PaymentMethod { get; set; } 
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Received;

        public int? CourierId { get; set; }
        public User? Courier { get; set; } 

        public DateTime PlacedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public DateTime? TakenAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
