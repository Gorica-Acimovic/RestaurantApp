using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Workers
{
    public class DailyStatisticsWorker : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyStatisticsWorker> _logger;

        public DailyStatisticsWorker(IServiceScopeFactory scopeFactory, ILogger<DailyStatisticsWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            do
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsService>();

                    var count = await statisticsService.CalculateMissingDaysAsync();
                    if (count > 0)
                    {
                        _logger.LogInformation("Daily statistics calculated for {Count} day(s).", count);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Daily statistics calculation failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
