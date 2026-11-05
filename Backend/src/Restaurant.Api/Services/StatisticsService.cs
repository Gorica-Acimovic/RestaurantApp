using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;

        public StatisticsService (AppDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        private static readonly Dictionary<DayOfWeek, int> BaseGuests = new()
        {
            [DayOfWeek.Monday] = 30,
            [DayOfWeek.Tuesday] = 35,
            [DayOfWeek.Wednesday] = 45,
            [DayOfWeek.Thursday] = 55,
            [DayOfWeek.Friday] = 90,
            [DayOfWeek.Saturday] = 110,
            [DayOfWeek.Sunday] = 75
        };

        public async Task<DailyStatistic> CalculateDayAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            var reservations = _context.Reservations. Where (r => r.StartsAt >= start && r.StartsAt < end 
                                && r.Status != ReservationStatus.Cancelled );

            var reservationCount = await reservations.CountAsync();
            var noShowCount = await reservations.CountAsync( r => r.Status == ReservationStatus.NoShow );

            var closedOrders = _context.Orders.Where( o => o.Status == OrderStatus.Closed &&
            o.Channel == OrderChannel.DineIn && o.ClosedAt >= start && o.ClosedAt < end );

            var orderCount = await closedOrders.CountAsync();

            var guestCount = await closedOrders.SumAsync(o => (int?)o.NumberOfGuests) ?? 0;

            var revenue = await _context.Invoices
                            .Where(i => i.IssuedAt >= start && i.IssuedAt < end )
                            .SumAsync(i => (decimal?)i.TotalAmount) ?? 0m;

            var now = _timeProvider.GetLocalNow().DateTime;

            var statistic = await _context.DailyStatistics.FirstOrDefaultAsync(d => d.Date == date);
            if ( statistic == null )
            {
                statistic = new DailyStatistic { Date = date };
                _context.DailyStatistics.Add( statistic );
            }
            Fill(statistic, reservationCount, guestCount, orderCount,noShowCount, revenue, now);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when ( ex.InnerException is SqlException  { Number: 2601 or 2627 })
            {
                _context.Entry(statistic).State = EntityState.Detached;
                statistic = await _context.DailyStatistics.FirstAsync(d => d.Date == date);

                Fill(statistic,reservationCount,guestCount,orderCount,noShowCount,revenue,now);
                await _context.SaveChangesAsync();
            }


           return statistic;
        }

        private static void Fill(DailyStatistic statistic, int reservationCount, int guestCount, int orderCount, int noShowCount, decimal revenue, DateTime now)
        {
            statistic.ReservationCount = reservationCount;
            statistic.GuestCount = guestCount;
            statistic.OrderCount = orderCount;
            statistic.NoShowCount = noShowCount;
            statistic.Revenue = revenue;
            statistic.IsSynthetic = false;
            statistic.GeneratedAt = now;

        }

        public async Task<int> CalculateMissingDaysAsync()
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            var yesterday = today.AddDays(-1);

            var firstReservation = await _context.Reservations.MinAsync(r => (DateTime?)r.StartsAt);
            var firstOrder = await _context.Orders.MinAsync(o => (DateTime?)o.OpenedAt);

            var firstActivity = new[] { firstReservation, firstOrder }
                  .Where(d => d != null)
                  .Min();

            if(firstActivity == null)
            {
                return 0;
            }

            var first = DateOnly.FromDateTime(firstActivity.Value);

            var calculated = (await _context.DailyStatistics
                .Where(d => d.Date >= first && d.Date <= yesterday)
                .Select(d => d.Date)
                .ToListAsync())
                .ToHashSet();

            var count = 0;
            for (var day = first; day <= yesterday; day = day.AddDays(1))
            {
                if (calculated.Contains(day))
                {
                    continue;
                }

                await CalculateDayAsync(day);
                count++;
            }

            return count;

        }


        public async Task<(int Count, DateOnly From, DateOnly To)> GenerateSyntheticHistoryAsync(int days)
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var today = DateOnly.FromDateTime(now);
            var to = today.AddDays(-1);
            var from = to.AddDays(-(days - 1));

            await _context.DailyStatistics
                .Where(d => d.IsSynthetic || (d.Date >= from && d.Date <= to))
                .ExecuteDeleteAsync();

            var random = new Random(42);
            var rows = new List<DailyStatistic>();

            for (var i = 0; i < days; i++)
            {
                var date = from.AddDays(i);

                var trend = 1 + 0.001 * i;
                var noise = 0.9 + random.NextDouble() * 0.2;
                var guests = (int)Math.Round(BaseGuests[date.DayOfWeek] * trend * noise);

                var orders = (int)Math.Round(guests / 2.6);
                var reservations = (int)Math.Round(orders * 0.7);
                var noShows = Math.Min(reservations, (int)Math.Round(reservations * 0.08 * (0.5 + random.NextDouble())));
                var revenue = Math.Round((decimal)(guests * 22.0 * (0.9 + random.NextDouble() * 0.2)), 2);

                rows.Add(new DailyStatistic
                {
                    Date = date,
                    ReservationCount = reservations,
                    GuestCount = guests,
                    OrderCount = orders,
                    NoShowCount = noShows,
                    Revenue = revenue,
                    IsSynthetic = true,
                    GeneratedAt = now
                });
            }

            _context.DailyStatistics.AddRange(rows);
            await _context.SaveChangesAsync();

            return (rows.Count, from, to);
        }

        public async Task<(StatisticsSummaryDto? Summary, string? Error, int StatusCode)> GetSummaryAsync(DateOnly? from, DateOnly? to)
        {
            var today = DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
            var end = to ?? today.AddDays(-1);
            var start = from ?? end.AddDays(-29);

            if (start > end)
            {
                return (null, "From date must not be after To date.", 400);
            }

            if (end.DayNumber - start.DayNumber > 730)
            {
                return (null, "The period can be at most 731 days.", 400);
            }

            var startTime = start.ToDateTime(TimeOnly.MinValue);
            var endTime = end.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var rows = await _context.DailyStatistics
                .Where(d => d.Date >= start && d.Date <= end)
                .OrderBy(d => d.Date)
                .ToListAsync();

            var totalReservations = rows.Sum(r => r.ReservationCount);
            var noShows = rows.Sum(r => r.NoShowCount);

            var weekdays = rows
                .GroupBy(r => r.Date.DayOfWeek)
                .Select(g => new WeekdayStatDto
                {
                    Day = g.Key.ToString(),
                    AverageGuests = Math.Round(g.Average(r => (decimal)r.GuestCount), 1)
                })
                .OrderByDescending(w => w.AverageGuests)
                .ToList();

            var hours = await _context.Orders
                .Where(o => o.Status == OrderStatus.Closed && o.Channel ==OrderChannel.DineIn && o.ClosedAt >= startTime && o.ClosedAt < endTime)
                .GroupBy(o => o.OpenedAt.Hour)
                .Select(g => new HourStatDto { Hour = g.Key, OrderCount = g.Count() })
                .OrderBy(h => h.Hour)
                .ToListAsync();

            var topItems = await _context.OrderItems
                .Where(i => i.Order.Status == OrderStatus.Closed && i.Order.ClosedAt >= startTime && i.Order.ClosedAt < endTime)
                .GroupBy(i => new { i.MenuItemId, i.MenuItem.Name })
                .Select(g => new TopItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    Name = g.Key.Name,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
                })
                .OrderByDescending(t => t.Quantity)
                .Take(5)
                .ToListAsync();

            var summary = new StatisticsSummaryDto
            {
                From = start,
                To = end,
                Days = rows.Count,
                ContainsSynthetic = rows.Any(r => r.IsSynthetic),
                TotalReservations = totalReservations,
                TotalGuests = rows.Sum(r => r.GuestCount),
                TotalOrders = rows.Sum(r => r.OrderCount),
                TotalRevenue = rows.Sum(r => r.Revenue),
                NoShowCount = noShows,
                NoShowPercent = totalReservations == 0 ? 0 : Math.Round(100m * noShows / totalReservations, 1),
                Weekdays = weekdays,
                Hours = hours,
                TopItems = topItems,
                Daily = rows.Select(r => new DailyPointDto
                {
                    Date = r.Date,
                    Guests = r.GuestCount,
                    Reservations = r.ReservationCount,
                    Revenue = r.Revenue,
                    IsSynthetic = r.IsSynthetic
                }).ToList()
            };

            return (summary, null, 200);
        }
    }
}
