using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.ML;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class ForecastService : IForecastService
    {
        private const int WindowSize = 14;
        private const int HoldoutDays = 14;
        private const int MaxFolds = 4;
        private const int MinTrainingDays = 42;
        private const int MinHistoryDays = 56;
        private const int BaselineWeeks = 4;

        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;

        public ForecastService(AppDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<(ForecastResultDto? Result, string? Error, int StatusCode)> GenerateAsync(int days)
        {
            if (days < 7 || days > 14)
            {
                return (null, "Days must be between 7 and 14.", 400);
            }

            var (history, error, statusCode) = await LoadHistoryAsync();
            if (history == null)
            {
                return (null, error, statusCode);
            }

            var values = history.Select(d => (double)d.GuestCount).ToList();
            var ssa = ForecastSsa(values, days);

            var dates = Enumerable.Range(1, days).Select(i => history[^1].Date.AddDays(i)).ToList();
            var baseline = dates.Select(d => WeekdayAverage(history, d)).ToList();

            var now = _timeProvider.GetLocalNow().DateTime;

            var saveError = await SaveAsync(dates, ssa, baseline, now);
            if (saveError != null)
            {
                return (null, saveError, 409);
            }

            var forecast = dates
                .Select((date, i) => new ForecastPointDto
                {
                    Date = date,
                    Ssa = Round(ssa[i]),
                    WeekdayAverage = Round(baseline[i])
                })
                .ToList();

            return (BuildResult(history, now, forecast), null, 200);
        }

        public async Task<(ForecastResultDto? Result, string? Error, int StatusCode)> GetLatestAsync()
        {
            var (history, error, statusCode) = await LoadHistoryAsync();
            if (history == null)
            {
                return (null, error, statusCode);
            }

            var latest = await _context.Predictions.MaxAsync(p => (DateTime?)p.GeneratedAt);
            if (latest == null)
            {
                return (null, "No forecast has been generated yet.", 404);
            }

            var stored = await _context.Predictions
                .Where(p => p.GeneratedAt == latest)
                .OrderBy(p => p.Date)
                .ToListAsync();

            var forecast = stored
                .GroupBy(p => p.Date)
                .Select(g => new ForecastPointDto
                {
                    Date = g.Key,
                    Ssa = g.FirstOrDefault(p => p.Model == ForecastModel.Ssa)?.ExpectedGuests ?? 0,
                    WeekdayAverage = g.FirstOrDefault(p => p.Model == ForecastModel.WeekdayAverage)?.ExpectedGuests ?? 0
                })
                .ToList();

            return (BuildResult(history, latest.Value, forecast), null, 200);
        }

        private async Task<(List<DailyStatistic>? History, string? Error, int StatusCode)> LoadHistoryAsync()
        {
            var rows = await _context.DailyStatistics.OrderBy(d => d.Date).ToListAsync();

            if (rows.Count < MinHistoryDays)
            {
                return (null, $"At least {MinHistoryDays} days of statistics are needed (found {rows.Count}).", 400);
            }

            if (rows[^1].Date.DayNumber - rows[0].Date.DayNumber + 1 != rows.Count)
            {
                return (null, "The statistics history has missing days.", 409);
            }

            return (rows, null, 200);
        }

        private ForecastResultDto BuildResult(List<DailyStatistic> history, DateTime generatedAt, List<ForecastPointDto> forecast)
        {
            var (metrics, backtest, folds, days) = RunBacktest(history);

            return new ForecastResultDto
            {
                GeneratedAt = generatedAt,
                TrainingFrom = history[0].Date,
                TrainingTo = history[^1].Date,
                TrainingDays = history.Count,
                ContainsSynthetic = history.Any(d => d.IsSynthetic),
                BacktestFolds = folds,
                BacktestDays = days,
                Metrics = metrics,
                Forecast = forecast,
                Backtest = backtest
            };
        }

        private (List<ForecastMetricDto> Metrics, List<BacktestPointDto> Points, int Folds, int Days) RunBacktest(List<DailyStatistic> history)
        {
            var n = history.Count;
            var folds = Math.Min(MaxFolds, (n - MinTrainingDays) / HoldoutDays);

            var actual = new List<double>();
            var ssaAll = new List<double>();
            var baselineAll = new List<double>();
            var lastFold = new List<BacktestPointDto>();

            for (var k = folds; k >= 1; k--)
            {
                var cut = n - HoldoutDays * k;
                var train = history.Take(cut).ToList();
                var test = history.Skip(cut).Take(HoldoutDays).ToList();

                var ssa = ForecastSsa(train.Select(d => (double)d.GuestCount).ToList(), HoldoutDays);

                for (var i = 0; i < test.Count; i++)
                {
                    var real = (double)test[i].GuestCount;
                    var average = WeekdayAverage(train, test[i].Date);

                    actual.Add(real);
                    ssaAll.Add(ssa[i]);
                    baselineAll.Add(average);

                    if (k == 1)
                    {
                        lastFold.Add(new BacktestPointDto
                        {
                            Date = test[i].Date,
                            Actual = test[i].GuestCount,
                            Ssa = Round(ssa[i]),
                            WeekdayAverage = Round(average)
                        });
                    }
                }
            }

            var metrics = new List<ForecastMetricDto>
            {
                new()
                {
                    Model = nameof(ForecastModel.Ssa),
                    Mae = Round(ForecastMetrics.Mae(actual, ssaAll)),
                    Mape = Round(ForecastMetrics.Mape(actual, ssaAll))
                },
                new()
                {
                    Model = nameof(ForecastModel.WeekdayAverage),
                    Mae = Round(ForecastMetrics.Mae(actual, baselineAll)),
                    Mape = Round(ForecastMetrics.Mape(actual, baselineAll))
                }
            };

            return (metrics, lastFold, folds, actual.Count);
        }

        private static double[] ForecastSsa(IReadOnlyList<double> values, int horizon)
        {
            var mlContext = new MLContext(seed: 42);
            var data = mlContext.Data.LoadFromEnumerable(values.Select(v => new SeriesPoint { Value = (float)v }));

            var pipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(SsaForecast.Forecast),
                inputColumnName: nameof(SeriesPoint.Value),
                windowSize: WindowSize,
                seriesLength: Math.Min(values.Count, 90),
                trainSize: values.Count,
                horizon: horizon);

            var model = pipeline.Fit(data);
            var engine = model.CreateTimeSeriesEngine<SeriesPoint, SsaForecast>(mlContext);

            return engine.Predict().Forecast.Select(v => Math.Max(0, (double)v)).ToArray();
        }

        private static double WeekdayAverage(IReadOnlyList<DailyStatistic> train, DateOnly date)
        {
            var sameWeekday = train
                .Where(d => d.Date.DayOfWeek == date.DayOfWeek)
                .TakeLast(BaselineWeeks)
                .ToList();

            return sameWeekday.Count > 0
                ? sameWeekday.Average(d => (double)d.GuestCount)
                : train.Average(d => (double)d.GuestCount);
        }

        private async Task<string?> SaveAsync(List<DateOnly> dates, double[] ssa, List<double> baseline, DateTime now)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var from = dates[0];
            var to = dates[^1];
            var existing = await _context.Predictions.Where(p => p.Date >= from && p.Date <= to).ToListAsync();

            for (var i = 0; i < dates.Count; i++)
            {
                Upsert(existing, dates[i], ForecastModel.Ssa, ssa[i], now);
                Upsert(existing, dates[i], ForecastModel.WeekdayAverage, baseline[i], now);
            }

            try
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return null;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return "Another forecast is being generated at the same time. Try again.";
            }
        }

        private void Upsert(List<Prediction> existing, DateOnly date, ForecastModel model, double value, DateTime now)
        {
            var prediction = existing.FirstOrDefault(p => p.Date == date && p.Model == model);
            if (prediction == null)
            {
                prediction = new Prediction { Date = date, Model = model };
                _context.Predictions.Add(prediction);
            }

            prediction.ExpectedGuests = Round(value);
            prediction.GeneratedAt = now;
        }

        private static decimal Round(double value) => Math.Round((decimal)value, 2);
    }
}