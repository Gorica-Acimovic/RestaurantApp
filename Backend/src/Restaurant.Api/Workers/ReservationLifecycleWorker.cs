using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Workers
{
    public class ReservationLifecycleWorker : BackgroundService
    {
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReservationLifecycleWorker> _logger;

        public ReservationLifecycleWorker(IServiceScopeFactory serviceScopeFactory, ILogger<ReservationLifecycleWorker> logger)
        {
            _serviceScopeFactory = serviceScopeFactory; 
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CheckInterval);

            do
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var reservationService = scope.ServiceProvider.GetRequiredService<IReservationService>();
                    var (noShows, cancelled) = await reservationService.CloseExpiredReservationsAsync();
                        if(noShows > 0 || cancelled > 0)
                    {
                        _logger.LogInformation("Reservations closed: {NoShows} no-show, {Cancelled} expired and cancelled.", noShows, cancelled);
                    }

                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Closing expired reservations failed.");        

                }

            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

    }
}
