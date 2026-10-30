namespace Restaurant.Api.DTOs
{
    public class WeekdayStatDto
    {
        public string Day { get; set; } = string.Empty;
        public decimal AverageGuests { get; set; }
    }
}
