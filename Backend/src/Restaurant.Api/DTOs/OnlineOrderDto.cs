namespace Restaurant.Api.DTOs
{
    public class OnlineOrderDto
    {
        public int Id { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime PlacedAt { get; set; }
        public string? Address { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CourierName { get; set; }
        public string? GuestName { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total { get; set; }
    }
}
