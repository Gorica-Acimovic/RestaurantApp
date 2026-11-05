using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Hubs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IHubContext<RestaurantHub> _hub;
        private readonly ILogger<KitchenService> _logger;

        public KitchenService(AppDbContext context, TimeProvider timeProvider, IHubContext<RestaurantHub> hub, ILogger<KitchenService> logger)
        {
            _context = context;
            _timeProvider = timeProvider;
            _hub = hub;
            _logger = logger;
        }

      
        public async Task<(string? Error, int StatusCode)> ChangeItemStatusAsync(int itemId, OrderItemStatus newStatus)
        {
            var orderId = await _context.OrderItems
                .Where(i => i.Id == itemId)
                .Select(i => (int?)i.OrderId)
                .FirstOrDefaultAsync();

            if (orderId == null)
            {
                return ("Order item not found.", 404);
            }

            return await ChangeAsync(orderId.Value, itemId, newStatus);
        }

        public async Task<(string? Error, int StatusCode)> ChangeOrderStatusAsync(int orderId, OrderItemStatus newStatus)
        {
            return await ChangeAsync(orderId, null, newStatus);
        }

        public async Task<List<KitchenTicketDto>> GetKitchenTicketAsync()
        {
            var orders = await _context.Orders
                 .Where(o => o.Status == OrderStatus.Open
                     && (o.Channel == OrderChannel.DineIn
                         || o.Delivery!.Status == DeliveryStatus.Confirmed
                         || o.Delivery!.Status == DeliveryStatus.Preparing)
                     && o.Items.Any(i => i.Status != OrderItemStatus.Ready))
                 .Include(o => o.Delivery)
                 .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                 .ToListAsync();

            return orders
                .Select(o => new KitchenTicketDto
                {
                    OrderId = o.Id,
                    Channel = o.Channel.ToString(),
                    Title = o.Channel == OrderChannel.DineIn ? $"Table {o.Table!.Number}" : $"{o.Channel} #{o.Id}",
                    Note = o.Delivery?.Note,
                    Since = o.Items.Where(i => i.Status != OrderItemStatus.Ready).Min(i => i.CreatedAt),
                    Items = o.Items
                        .OrderBy(i => i.Id)
                        .Select(i => new KitchenItemDto
                        {
                            Id = i.Id,
                            Name = i.MenuItem.Name,
                            Quantity = i.Quantity,
                            Note = i.Note,
                            Status = i.Status.ToString()
                        })
                        .ToList()

                }).OrderBy(t => t.Since)
                .ToList();

        }

        private async Task<(string? Error, int StatusCode)> ChangeAsync (int orderId, int? itemId, OrderItemStatus newStatus)
        {
            if(newStatus == OrderItemStatus.New)
            {
                return ("Items can only move forward: Preparing -> Ready", 400);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE Id = {orderId}");

            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Delivery)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId );

            if (order == null)
            {
                return("Order not found", 404);
            }

            if(order.Status != OrderStatus.Open)
            {
                return ("Order is not open", 409);
            }

            var delivery = order.Delivery;
            if(delivery != null && delivery.Status!= DeliveryStatus.Confirmed && delivery.Status != DeliveryStatus.Preparing)
            {
                return ("Delivery is not confirmed or preparing", 409);
            }

            if(itemId != null && order.Items.All(i => i.Id != itemId))
            {
                return ("Order item not found.", 404);
            }

            var targets = order.Items
               .Where(i => (itemId == null || i.Id == itemId) && i.Status < newStatus)
               .ToList();

            if (targets.Count == 0)
            {
                return ("Nothing to update, items are already at this stage.", 409);
            }

            foreach (var item in targets)
            {
                item.Status = newStatus;
            }

            var allReady = order.Items.All(i => i.Status == OrderItemStatus.Ready);

            if (delivery != null)
            {
                if (allReady)
                {
                    delivery.Status = DeliveryStatus.Ready;
                    delivery.ReadyAt = _timeProvider.GetLocalNow().DateTime;
                }
                else if (delivery.Status == DeliveryStatus.Confirmed)
                {
                    delivery.Status = DeliveryStatus.Preparing;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Kitchen moved {Count} item(s) of order {OrderId} to {Status}", targets.Count, orderId, newStatus);

            await _hub.Clients.Group("kitchen").SendAsync("KitchenChanged");

            if (delivery != null)
            {
                await _hub.Clients.Groups("managers", "couriers", $"user-{order.GuestId}").SendAsync("OnlineOrdersChanged");
            }
            else
            {
                await _hub.Clients.Group($"table-{order.TableId}").SendAsync("OrderChanged");

                if (allReady)
                {
                    await _hub.Clients.Group($"user-{order.WaiterId}").SendAsync("OrderReady", order.Table!.Number);
                }
            }

            return (null, 204);
        }
    }
}
