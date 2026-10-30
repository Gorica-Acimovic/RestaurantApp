using Restaurant.Api.DTOs;

namespace Restaurant.Api.Interfaces
{
    public interface IForecastService
    {
        Task<(ForecastResultDto? Result, string? Error, int StatusCode)> GenerateAsync(int days);
        Task<(ForecastResultDto? Result, string? Error, int StatusCode)> GetLatestAsync();
    }
}