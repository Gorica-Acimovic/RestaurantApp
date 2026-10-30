namespace Restaurant.Api.Models
{
    public class Prediction
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public decimal ExpectedGuests { get; set; }
        public ForecastModel Model { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
