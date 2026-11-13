namespace Restaurant.Api.DTOs
{
    public class KitchenTicketDto
    {
        public int OrderId { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime Since { get; set; } 
        public List<KitchenItemDto> Items { get; set; } = new();
    }
}
