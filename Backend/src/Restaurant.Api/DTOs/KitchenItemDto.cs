namespace Restaurant.Api.DTOs
{
    public class KitchenItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Note { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
