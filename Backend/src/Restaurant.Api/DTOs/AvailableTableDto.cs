namespace Restaurant.Api.DTOs
{
    public class AvailableTableDto
    {
        public int Id { get; set; }
        public int Number {  get; set; }
        public int Capacity { get; set; }
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public bool IsAvailable { get; set; }
    }
}
