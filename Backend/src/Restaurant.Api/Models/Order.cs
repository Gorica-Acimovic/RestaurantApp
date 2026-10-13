namespace Restaurant.Api.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Open;

        public int TableId { get; set; }
        public RestaurantTable Table { get; set; } = null!;

        public int WaiterId { get; set; }
        public User Waiter { get; set; } = null!;

        public int NumberOfGuests { get; set; }

        public List<OrderItem> Items { get; set; } = new();
        public Invoice? Invoice { get; set; }

    }
}
