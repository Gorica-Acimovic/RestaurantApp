namespace Restaurant.Api.DTOs
{
    public class BacktestPointDto
    {
        public DateOnly Date { get; set; }
        public decimal Actual { get; set; }
        public decimal Ssa { get; set; }
        public decimal WeekdayAverage { get; set; }
    }
}
