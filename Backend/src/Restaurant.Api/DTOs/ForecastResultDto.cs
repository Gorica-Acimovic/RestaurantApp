namespace Restaurant.Api.DTOs
{
    public class ForecastResultDto
    {
        public DateTime GeneratedAt { get; set; }

        public DateOnly TrainingFrom { get; set; }
        public DateOnly TrainingTo { get; set; }
        public int TrainingDays { get; set; }
        public bool ContainsSynthetic { get; set; }

        public int BacktestFolds { get; set; }
        public int BacktestDays { get; set; }

        public List<ForecastMetricDto> Metrics { get; set; } = new();
        public List<ForecastPointDto> Forecast { get; set; } = new();
        public List<BacktestPointDto> Backtest { get; set; } = new();
    }
}
