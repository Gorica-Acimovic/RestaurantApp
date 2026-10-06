using Restaurant.Api.Models;

namespace Restaurant.Api.DTOs
{
    public class TableDto
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
    }
}
