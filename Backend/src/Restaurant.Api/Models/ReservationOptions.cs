namespace Restaurant.Api.Models
{
    public class ReservationOptions
    {
        public int DurationMinutes { get; set; } = 120;
        public int OpeningHour { get; set; } = 10;
        public int ClosingHour { get; set; } = 23;
        public int MaxDaysAhead { get; set; } = 30;
        public int MinCancelHoursBefore { get; set; } = 2;
        public int ReservedSoonMinutes { get; set; } = 30;
    }
}
