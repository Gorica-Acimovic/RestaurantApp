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

            if (next == ReservationStatus.Confirmed && reservation.StartsAt <= now)
            {
                return (null, "The reservation time has already passed. It can not be confirmed.", 400);
            }

            if (next == ReservationStatus.NoShow && reservation.StartsAt > now)
            {
                return (null, "A reservation can be marked as a no-show only after its start time.", 400);
            }

            if (next == ReservationStatus.Completed && reservation.StartsAt - now > TimeSpan.FromMinutes(_reservationOptions.ReservedSoonMinutes))
            {
                return (null, "A reservation can be marked as completed only when the guests arrive.", 400);
            }

            if (!isStaff && reservation.StartsAt - now < TimeSpan.FromHours(_reservationOptions.MinCancelHoursBefore))
            {
                return (null, $"Reservation can be cancelled at least {_reservationOptions.MinCancelHoursBefore} hours before it starts.", 400);
            }

            RestaurantTable? table = null;

            if (next == ReservationStatus.Confirmed)
            {
                var tableId = request.TableId ?? reservation.TableId;                                                             
                                                                     
                if (tableId == null)
                {
                    return (null, "Choose a table before confirming the reservation.", 400);
                }

                table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == tableId && t.IsActive);

                if (table == null)
                {
                    return (null, "Table not found", 400);
                }

                if (table.Capacity < reservation.NumberOfGuests)
                {
                    return (null, "The table is too small for that number of guests.", 400);
                }

            }


            try
            {
                _context.Entry(reservation).Property(r => r.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return (null, "Invalid row version.", 400);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (table != null)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync
                        ($"SELECT Id FROM RestaurantTables WITH(UPDLOCK, ROWLOCK) WHERE Id ={table.Id}");

                    var taken = await _context.Reservations.AnyAsync(r => r.Id != reservation.Id && r.TableId == table.Id &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                        && r.StartsAt < reservation.EndsAt && r.EndsAt > reservation.StartsAt);

                    if (taken)
                    {
                        return (null, "The table is already reserved for that time.", 409);
                    }

                    reservation.Table = table;
                }

                reservation.Status = next;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return (null, "Reservation was changed by someone else. Reload and try again.", 409);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return (null, "The table is already reserved for that time.", 409);
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
            var startTime = request.StartsAt;
            var endTime = startTime.AddMinutes(_reservationOptions.DurationMinutes);

            var timeError = ValidateTime(startTime, endTime);

            if (timeError != null)
            {
                return (null, timeError, false);
            }

            RestaurantTable? table = null;

            if (request.TableId != null)
            {
                table = await _context.RestaurantTables.
                    FirstOrDefaultAsync(t => t.Id == request.TableId && t.IsActive);
                if (table == null)
                {
                    return (null, "Table not found", false);
                }

                if (table.Capacity < request.NumberOfGuests)
                {
                    return (null, "The table is too small for that number of guests.", false);
                }

            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {

                if (table != null)
                {
                    await _context.Database.ExecuteSqlInterpolatedAsync
                        ($"SELECT Id FROM RestaurantTables WITH(UPDLOCK, ROWLOCK) WHERE Id ={table.Id}");

                    var taken = await _context.Reservations.AnyAsync(r => r.TableId == table.Id &&
                  (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                  && r.StartsAt < endTime && r.EndsAt > startTime);

                    if (taken)
                    {
                        return (null, "The table is already reserved for that time.", true);
                    }


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
                    TableId = table?.Id,
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
                    TableId = table?.Id,
                    TableNumber = table?.Number,
                    RowVersion = Convert.ToBase64String(reservation.RowVersion)
                };

                return (dto, null, false);
            }

            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return (null, "No free table for that time.", true);
            }
        }

        public async Task<List<ReservationDto>> GetReservationsAsync(DateTime? date)
        {
            var query = _context.Reservations.AsQueryable();

            if (date != null)
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

        public async Task<List<ReservationDto>> GetMyReservationsAsync(int userId)
        {
            return await _context.Reservations
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.StartsAt)
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
                    RowVersion = Convert.ToBase64String(r.RowVersion),
                    HasReview = r.Review != null
                })
                .ToListAsync();
        }

        public async Task<(int NoShows, int Cancelled)> CloseExpiredReservationsAsync()
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var noShowLimit = now.AddMinutes(-_reservationOptions.NoShowAfterMinutes);

            var noShows = await _context.Reservations.Where(r => r.Status == ReservationStatus.Confirmed &&
            r.StartsAt <= noShowLimit).ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.NoShow));

            var cancelled = await _context.Reservations.Where(r => r.Status == ReservationStatus.Pending &&
            r.StartsAt <= now).ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReservationStatus.Cancelled));

            if (noShows + cancelled > 0)
            {
                await _hub.Clients.All.SendAsync("ReservationsChanged");
            }

            return (noShows, cancelled);
        }

        public async Task<(List<AvailableTableDto>? Tables, string? Error)> GetAvailableTablesAsync(DateTime startsAt, int guestNumber)
        {
            if (guestNumber < 1 || guestNumber > 50)
            {
                return (null, "Number of guests must be between 1 and 50.");
            }

            var endsAt = startsAt.AddMinutes(_reservationOptions.DurationMinutes);

            var timeError = ValidateTime(startsAt, endsAt);

            if (timeError != null)
            {
                return (null, timeError);
            }

            var takenTableIds = await _context.Reservations
                .Where(r => r.TableId != null && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                && r.StartsAt < endsAt && r.EndsAt > startsAt).Select(r => r.TableId!.Value).ToListAsync();

            var tables = await _context.RestaurantTables.Where(t => t.IsActive && t.Capacity >= guestNumber)
                .OrderBy(t => t.Number)
                .Select(t => new AvailableTableDto
                {
                    Id = t.Id,
                    Number = t.Number,
                    Capacity = t.Capacity,
                    PositionX = t.PositionX,
                    PositionY = t.PositionY,
                    IsAvailable = !takenTableIds.Contains(t.Id) 

                }).ToListAsync();

            return (tables, null);

        }



        private string? ValidateTime(DateTime startTime, DateTime endTime)
        {
            var now = _timeProvider.GetLocalNow().DateTime;

            if (startTime <= now)
            {
                return "Reservation time cannot be in the past.";
            }

            if (startTime > now.AddDays(_reservationOptions.MaxDaysAhead))
            {
                return $"Reservation time cannot be more than {_reservationOptions.MaxDaysAhead} in the future.";
            }

            if (startTime.TimeOfDay < TimeSpan.FromHours(_reservationOptions.OpeningHour) || endTime > startTime.Date.AddHours(_reservationOptions.ClosingHour))
            {
                return $"Reservation time must be between {_reservationOptions.OpeningHour}:00 and {_reservationOptions.ClosingHour}:00.";
            }

            return null;
        }

        public async Task<(AvailableTableDto? Table, string? Error, int StatusCode)> SuggestTableAsync(int id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return (null, "Reservation not found.", 404);
            }

            var takenTableIds = await _context.Reservations
                    .Where(r => r.Id != id && r.TableId != null
                    && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                    && r.StartsAt < reservation.EndsAt && r.EndsAt > reservation.StartsAt)
                    .Select(r => r.TableId!.Value)
                    .ToListAsync();

            var table = await _context.RestaurantTables
                    .Where(t => t.IsActive && t.Capacity >= reservation.NumberOfGuests &&
                    !takenTableIds.Contains(t.Id))
                    .OrderBy(t => t.Capacity)
                    .ThenBy(t => t.Number)
                    .Select(t => new AvailableTableDto
                    {
                        Id = t.Id,
                        Number = t.Number,
                        Capacity = t.Capacity,
                        PositionX = t.PositionX,
                        PositionY = t.PositionY,
                        IsAvailable = true

                    }).FirstOrDefaultAsync();

            if (table == null)
            {
                return (null, "There is no free table for that reservation.", 404);
            }

            return (table, null, 200);

        }
    }
}
