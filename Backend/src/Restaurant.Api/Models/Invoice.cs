namespace Restaurant.Api.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public DateTime IssuedAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
