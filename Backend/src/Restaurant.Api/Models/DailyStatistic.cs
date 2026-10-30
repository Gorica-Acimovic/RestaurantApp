namespace Restaurant.Api.Models
{
    public class DailyStatistic
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }

        public int ReservationCount { get; set; }
        public int GuestCount { get; set; } 
        public int OrderCount { get; set; }
        public int NoShowCount { get; set; }
        public decimal Revenue { get; set; }

        public bool IsSynthetic { get; set; }
        public DateTime GeneratedAt { get; set; }

    }
}
