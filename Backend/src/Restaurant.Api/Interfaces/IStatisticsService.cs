using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface IStatisticsService
    {
        Task<DailyStatistic> CalculateDayAsync(DateOnly date);
        Task<int> CalculateMissingDaysAsync();
        Task<(int Count, DateOnly From, DateOnly To)> GenerateSyntheticHistoryAsync(int days);
        Task<(StatisticsSummaryDto? Summary, string? Error, int StatusCode)> GetSummaryAsync(DateOnly? from, DateOnly? to);
    }
}
