namespace Restaurant.Api.Models
{
    public class RestaurantTable
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Available;
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public bool IsActive { get; set; } = true;

    }
}
