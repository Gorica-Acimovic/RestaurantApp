using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles="Admin, Manager")]
    public class StatisticsController : ApiControllerBase
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IForecastService _forecastService;

        public StatisticsController(IStatisticsService statisticsService, IForecastService forecastService)
        {
            _statisticsService = statisticsService;
            _forecastService = forecastService; 
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromQuery] DateOnly? date)
        {
            if (date == null)
            {
                return Error(400, "Date is required (format yyyy-MM-dd).");
            }

            var statistic = await _statisticsService.CalculateDayAsync(date.Value);
            return Ok(statistic);
        }

        [HttpPost("synthetic")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateSynthetic([FromQuery] int days = 180)
        {
            if (days < 60 || days > 730)
            {
                return Error(400, "Days must be between 60 and 730.");
            }

            var (count, from, to) = await _statisticsService.GenerateSyntheticHistoryAsync(days);
            return Ok(new { count, from, to });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
        {
            var (summary, error, statusCode) = await _statisticsService.GetSummaryAsync(from, to);
            if (summary == null)
            {
                return Error(statusCode, error);
            }

            return Ok(summary);
        }

        [HttpPost("forecast")]
        public async Task<IActionResult> GenerateForecast([FromQuery] int days = 14)
        {
            var (result, error, statusCode) = await _forecastService.GenerateAsync(days);
            if (result == null)
            {
                return Error(statusCode, error);
            }

            return Ok(result);
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> GetForecast()
        {
            var (result, error, statusCode) = await _forecastService.GetLatestAsync();
            if (result == null)
            {
                return Error(statusCode, error);
            }

            return Ok(result);
        }

    }
}
