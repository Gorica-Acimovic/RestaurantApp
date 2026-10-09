using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Hubs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _context;
        private readonly ReservationOptions _reservationOptions;
        private readonly TimeProvider _timeProvider;
        IHubContext<RestaurantHub> _hub;

        public ReservationService(AppDbContext context, IOptions<ReservationOptions> reservationOptions, TimeProvider timeProvider, IHubContext<RestaurantHub> hub)
        {
            _context = context;
            _reservationOptions = reservationOptions.Value;
            _timeProvider = timeProvider;
            _hub = hub;
        }

        public async Task<(ReservationDto? Reservation, string? Error, int StatusCode)> ChangeStatusAsync(int id, ChangeReservationStatusRequest request, int userId, bool isStaff)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return (null, "Reservation not found.", 404);
            }

            var isOwner = reservation.UserId == userId;
            if (!isStaff && !(isOwner && request.Status == ReservationStatus.Cancelled))
            {
                return (null, "You are not allowed to do this.", 403);
            }

            var current = reservation.Status;
            var next = request.Status;
            var allowed =
                (current == ReservationStatus.Pending && (next == ReservationStatus.Confirmed || next == ReservationStatus.Cancelled)) ||
                (current == ReservationStatus.Confirmed && (next == ReservationStatus.Completed || next == ReservationStatus.NoShow || next == ReservationStatus.Cancelled));

            if (!allowed)
            {
                return (null, $"Status cannot be changed from {current} to {next}.", 400);
            }

            var now = _timeProvider.GetLocalNow().DateTime;
            if (!isStaff && reservation.StartsAt - now < TimeSpan.FromHours(_reservationOptions.MinCancelHoursBefore))
            {
                return (null, $"Reservation can be cancelled at least {_reservationOptions.MinCancelHoursBefore} hours before it starts.", 400);
            }

            try
            {
                _context.Entry(reservation).Property(r => r.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return (null, "Invalid row version.", 400);
            }

            reservation.Status = next;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return (null, "Reservation was changed by someone else. Reload and try again.", 409);
            }
            await _hub.Clients.All.SendAsync("ReservationsChanged");

            var dto = new ReservationDto
            {
                Id = reservation.Id,
                CustomerName = reservation.CustomerName,
                CustomerEmail = reservation.CustomerEmail,
                CustomerPhone = reservation.CustomerPhone,
                StartsAt = reservation.StartsAt,
                EndsAt = reservation.EndsAt,
                NumberOfGuests = reservation.NumberOfGuests,
                Status = reservation.Status.ToString(),
                TableId = reservation.TableId,
                TableNumber = reservation.Table?.Number,
                RowVersion = Convert.ToBase64String(reservation.RowVersion)
            };

            return (dto, null, 200);
        }

        public async Task<(ReservationDto? Reservation, string? Error, bool Conflict)> CreateReservationAsync(ReservationRequest request, int? userId)
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var startTime = request.StartsAt;
            var endTime = startTime.AddMinutes(_reservationOptions.DurationMinutes);

            if(startTime <= now)
            {
                return (null, "Reservation time cannot be in the past.", false);
            }

            if(startTime > now.AddDays(_reservationOptions.MaxDaysAhead))
            {
                return (null, $"Reservation time cannot be more than {_reservationOptions.MaxDaysAhead} days in the future.", false);
            }

            if(startTime.TimeOfDay < TimeSpan.FromHours(_reservationOptions.OpeningHour) || endTime > startTime.Date.AddHours(_reservationOptions.ClosingHour))
            {
                return (null, $"Reservation time must be between {_reservationOptions.OpeningHour}:00 and {_reservationOptions.ClosingHour}:00.", false);
            }

            var possibleTables = await _context.RestaurantTables
                .Where(t => t.IsActive && t.Capacity >= request.NumberOfGuests)
                .OrderBy(t => t.Capacity)
                .ThenBy(t => t.Number)
                .ToListAsync();

            if(possibleTables.Count == 0)
            {
                return (null, "No available tables for the requested number of guests.", false);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var table in possibleTables)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM RestaurantTables WITH(UPDLOCK, ROWLOCK) WHERE Id ={table.Id}");
                    var taken = await _context.Reservations.AnyAsync(r => r.TableId == table.Id &&
                    (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                    && r.StartsAt < endTime && r.EndsAt > startTime);

                    if (taken)
                    {
                        continue;
                    }

                    var reservation = new Reservation
                    {
                        CustomerName = request.CustomerName,
                        CustomerEmail = request.CustomerEmail,
                        CustomerPhone = request.CustomerPhone,
                        StartsAt = startTime,
                        EndsAt = endTime,
                        NumberOfGuests = request.NumberOfGuests,
                        UserId = userId,
                        TableId = table.Id,
                    };

                    _context.Reservations.Add(reservation);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await _hub.Clients.All.SendAsync("ReservationsChanged");

                    var dto = new ReservationDto
                    {
                        Id = reservation.Id,
                        CustomerName = reservation.CustomerName,
                        CustomerEmail = reservation.CustomerEmail,
                        CustomerPhone = reservation.CustomerPhone,
                        StartsAt = reservation.StartsAt,
                        EndsAt = reservation.EndsAt,
                        NumberOfGuests = reservation.NumberOfGuests,
                        Status = reservation.Status.ToString(),
                        TableId = table.Id,
                        TableNumber = table.Number,
                        RowVersion = Convert.ToBase64String(reservation.RowVersion)
                    };

                    return (dto, null, false);
                }

                return (null, "No free table for that time.", true);
            }

            catch(DbUpdateException ex) when( ex.InnerException is SqlException {Number: 2601 or 2627 })
            {
                return (null, "No free table for that time.", true);
            }
        }

        public async Task<List<ReservationDto>> GetReservationsAsync(DateTime? date)
        {
           var query = _context.Reservations.AsQueryable();

            if(date != null)
            {
                var from = date.Value.Date;
                var to = from.AddDays(1);
                query = query.Where(r => r.StartsAt >= from && r.StartsAt < to);

            }
            return await query
                .OrderBy(r => r.StartsAt)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    CustomerName = r.CustomerName,
                    CustomerEmail = r.CustomerEmail,
                    CustomerPhone = r.CustomerPhone,
                    StartsAt = r.StartsAt,
                    EndsAt = r.EndsAt,
                    NumberOfGuests = r.NumberOfGuests,
                    Status = r.Status.ToString(),
                    TableId = r.TableId,
                    TableNumber = r.Table != null ? r.Table.Number : null,
                    RowVersion = Convert.ToBase64String(r.RowVersion)

                }).ToListAsync();

        }
    }
}
