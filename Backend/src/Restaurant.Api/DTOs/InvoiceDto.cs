namespace Restaurant.Api.DTOs
{
    public class InvoiceDto
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public string InvoiceNumberText { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public int TableNumber { get; set; }
        public string WaiterName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}
