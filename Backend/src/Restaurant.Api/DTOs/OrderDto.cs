namespace Restaurant.Api.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int TableId { get; set; }
        public int TableNumber { get; set; }
        public string WaiterName { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal Total { get; set; }
    }
}
