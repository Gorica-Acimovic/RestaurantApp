using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Restaurant.Api.Hubs;

namespace Restaurant.Api.Services
{
    public class TableService : ITableService
    {
        private readonly AppDbContext _context;
        private readonly ReservationOptions _reservationOptions;
        private readonly TimeProvider _timeProvider;
        private readonly IHubContext<RestaurantHub> _hub;
            

        public TableService(AppDbContext context, IOptions<ReservationOptions> reservationOptions, TimeProvider timeProvider, IHubContext<RestaurantHub> hub)
        {
            _context = context;
            _reservationOptions = reservationOptions.Value;
            _timeProvider = timeProvider;
            _hub = hub;
        }
        public async Task<(TableDto? Table, string? Error)> CreateTableAsync(SaveTableRequest request)
        {
           var tableTaken = await _context.RestaurantTables.AnyAsync(t => t.Number == request.Number && t.IsActive);

            if (tableTaken)
            {
                return (null, "Table number is already taken.");
            }

            var table = new RestaurantTable
            {
                Number = request.Number,
                Capacity = request.Capacity,
                PositionX = request.PositionX,
                PositionY = request.PositionY,
               
            };

            _context.RestaurantTables.Add(table);
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TablesChanged");

            var tableDto = new TableDto
            {
                Id = table.Id,
                Number = table.Number,
                Capacity = table.Capacity,
                Status = table.Status.ToString(),
                PositionX = table.PositionX,
                PositionY = table.PositionY
            };

            return (tableDto, null);
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            if(table == null)
            {
                return false;
            }
            table.IsActive = false;
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TablesChanged");
            return true;
        }

        public async Task<List<TableDto>> GetAllTablesAsync()
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var limit = now.AddMinutes(_reservationOptions.ReservedSoonMinutes);

            var tables = await _context.RestaurantTables
                .Where(t =>t.IsActive)
                .OrderBy(t=>t.Number)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    Number = t.Number,
                    Capacity = t.Capacity,
                    Status = t.Status.ToString(),
                    PositionX = t.PositionX,
                    PositionY = t.PositionY,
                    ReservedSoon = _context.Reservations.Any(r =>
                        r.TableId == t.Id &&
                        r.Status == ReservationStatus.Confirmed &&
                        r.StartsAt >= now &&
                        r.StartsAt <= limit)
                })
                .ToListAsync();

            return tables;
        }

        public async Task<bool> UpdatePositionOfTableAsync(int id, UpdateTablePositionRequest request)
        {
            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            if(table == null) { 
                return false;
            }
            table.PositionX = request.PositionX;
            table.PositionY = request.PositionY;
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TablesChanged");
            return true;
        }

        public async Task<(bool Found, string? Error)> UpdateStatusAsync(int id, UpdateTableStatusRequest request)
        {
            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            if (table == null)
            {
                return (false, "Table not found.");
            }

            if (request.Status == TableStatus.Available)
            {
                var hasOpenOrder = await _context.Orders.AnyAsync(o => o.TableId == id && o.Status == OrderStatus.Open);
                if (hasOpenOrder)
                {
                    return (true, "Table has an open order. Close or cancel the order first.");
                }
            }

            table.Status = request.Status;
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TablesChanged");
            return (true, null);
        }

        public async Task<(bool Found, string? Error)> UpdateTableAsync(int id, SaveTableRequest request)
        {
            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
            if(table == null)
            {
                return (false, "Table not found.");
            }

            var tableNumber = await _context.RestaurantTables.AnyAsync(t => t.Number == request.Number && t.Id != id && t.IsActive);
            if (tableNumber)
            {
                return (true, "Table number is already taken.");
            }

            table.Number = request.Number;
            table.Capacity = request.Capacity;
            table.PositionX = request.PositionX;
            table.PositionY = request.PositionY;
            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("TablesChanged");
            return (true, null);

        }
    }
}
