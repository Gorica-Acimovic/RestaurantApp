namespace Restaurant.Api.DTOs
{
    public class StatisticsSummaryDto
    {
        public DateOnly From { get; set; }
        public DateOnly To { get; set; }
        public int Days { get; set; }
        public bool ContainsSynthetic { get; set; }

        public int TotalReservations { get; set; }
        public int TotalGuests { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NoShowCount { get; set; }
        public decimal NoShowPercent { get; set; }

        public List<WeekdayStatDto> Weekdays { get; set; } = new();
        public List<HourStatDto> Hours { get; set; } = new();
        public List<TopItemDto> TopItems { get; set; } = new();
        public List<DailyPointDto> Daily { get; set; } = new();
    }
}
