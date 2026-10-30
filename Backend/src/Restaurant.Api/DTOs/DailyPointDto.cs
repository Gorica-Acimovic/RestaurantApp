namespace Restaurant.Api.DTOs
{
    public class DailyPointDto
    {
        public DateOnly Date { get; set; }
        public int Guests { get; set; }
        public int Reservations { get; set; }
        public decimal Revenue { get; set; }
        public bool IsSynthetic { get; set; }
    }
}
