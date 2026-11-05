using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Hubs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using Microsoft.Data.Sql;
using Microsoft.Data.SqlClient;

namespace Restaurant.Api.Services
{
    public class OnlineOrderService : IOnlineOrderService
    {
        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly OnlineOrderOptions _options;
        private readonly ILogger<OnlineOrderService> _logger;
        private readonly IHubContext<RestaurantHub> _hub;

        private static readonly Dictionary<DeliveryStatus, DeliveryStatus[]> ManagerTransitions = new()
        {
            [DeliveryStatus.Received] = new[] { DeliveryStatus.Confirmed, DeliveryStatus.Cancelled },
            [DeliveryStatus.Confirmed] = new[] { DeliveryStatus.Preparing, DeliveryStatus.Cancelled },
            [DeliveryStatus.Preparing] = new[] { DeliveryStatus.Ready, DeliveryStatus.Cancelled },
            [DeliveryStatus.Ready] = new[] { DeliveryStatus.PickedUp, DeliveryStatus.Cancelled },
            [DeliveryStatus.OutForDelivery] = new[] { DeliveryStatus.Cancelled }

        };

        public OnlineOrderService(AppDbContext context, TimeProvider timeProvider, IOptions<OnlineOrderOptions> options, ILogger<OnlineOrderService> logger, IHubContext<RestaurantHub> hub )
        {
            _context = context;
            _timeProvider = timeProvider;
            _options = options.Value;
            _logger = logger;
            _hub = hub;
        }


        public async Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> CreateAsync(CreateOnlineOrderRequest request, int guestId)
        {
            if(request.Channel == OrderChannel.DineIn)
            {
                return (null, "Online order can be for pickup or delivery only.", 400);
            }
            
            if(request.Channel == OrderChannel.Delivery && string.IsNullOrWhiteSpace(request.Address))
            {
                return (null, "Address is required for delivery orders.", 400);
            }

            if(request.Items.Count > _options.MaxLinesPerOrder)
            {
                return (null, $"Order can have at most {_options.MaxLinesPerOrder} items.", 400);
            }

            var menuIds = request.Items.Select(i => i.MenuItemId).Distinct().ToList();
            var menuItems = await _context.MenuItems.Where(mi => menuIds.Contains(mi.Id) && mi.IsActive).ToDictionaryAsync(mi => mi.Id);

            foreach(var line in request.Items)
            {
                if(!menuItems.TryGetValue(line.MenuItemId, out var menuItem))
                {
                    return (null, $"Menu item with ID {line.MenuItemId} does not exist or is not active.", 400);
                }

               if(!menuItem.IsAvailable)
                {
                    return (null, $"Menu item '{menuItem.Name}' is not available for ordering.", 400);
                }
            }

            await using var transactions = await _context.Database.BeginTransactionAsync();
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT Id FROM Users WITH (UPDLOCK, ROWLOCK) WHERE Id = {guestId}");

            var activeOrders = await _context.Orders.CountAsync(o =>
                o.GuestId == guestId && o.Channel != OrderChannel.DineIn && o.Status == OrderStatus.Open);

            if (activeOrders >= _options.MaxActiveOrdersPerGuest)
            {
                _logger.LogWarning("Online order rejected for guest {GuestId}, {Active} active orders already", guestId, activeOrders);
                return (null, $"You already have {activeOrders} active orders. Wait until one is completed.", 409);
            }

            var now = _timeProvider.GetLocalNow().DateTime;
            var isDelivery = request.Channel == OrderChannel.Delivery;

            var order = new Order
            {
                Channel = request.Channel,
                GuestId = guestId,
                OpenedAt = now,
                NumberOfGuests = 1,
                Items = request.Items.Select(line => new OrderItem
                {
                    MenuItem = menuItems[line.MenuItemId],
                    Quantity = line.Quantity,
                    UnitPrice = menuItems[line.MenuItemId].Price,
                    Note = CleanNote(line.Note),
                    CreatedAt = now
                }).ToList(),
                Delivery = new Delivery
                {
                    Address = isDelivery ? request.Address!.Trim() : null,
                    Phone = request.Phone.Trim(),
                    Note = CleanNote(request.Note),
                    DeliveryFee = isDelivery ? _options.DeliveryFee : 0m,
                    PaymentMethod = request.PaymentMethod,
                    PlacedAt = now
                }
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transactions.CommitAsync();
            await NotifyAsync(order);

            _logger.LogInformation("Online order {OrderId} placed by guest {GuestId}, {Channel}, {Lines} line(s)", order.Id, guestId, request.Channel, request.Items.Count);

            return (ToDto(order), null, 200);
        }

        public async Task<List<OnlineOrderDto>> GetMineAsync(int guestId)
        {
            var orders = await _context.Orders
                .Where(o => o.GuestId == guestId && o.Channel != OrderChannel.DineIn)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .Include(o => o.Delivery).ThenInclude(d => d!.Courier)
                .OrderByDescending(o => o.OpenedAt)
                .Take(50)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }


        public async Task<List<OnlineOrderDto>> GetActiveAsync()
        {
            var orders = await _context.Orders.Where(o => o.Channel != OrderChannel.DineIn && o.Status == OrderStatus.Open)
                .Include(o => o.Guest)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .Include(o => o.Delivery).ThenInclude(d => d!.Courier)
                .OrderBy(o => o.OpenedAt)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }



        private static string? CleanNote(string? note)
        {
            return string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        }

        private static OnlineOrderDto ToDto(Order order)
        {
            var items = order.Items
                .OrderBy(i => i.Id)
                .Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    MenuItemId = i.MenuItemId,
                    MenuItemName = i.MenuItem.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice,
                    Note = i.Note,
                    Status = i.Status.ToString()
                })
                .ToList();

            var delivery = order.Delivery!;
            var subtotal = items.Sum(i => i.LineTotal);

            return new OnlineOrderDto
            {
                Id = order.Id,
                Channel = order.Channel.ToString(),
                Status = delivery.Status.ToString(),
                PlacedAt = delivery.PlacedAt,
                Address = delivery.Address,
                Phone = delivery.Phone,
                Note = delivery.Note,
                PaymentMethod = delivery.PaymentMethod.ToString(),
                CourierName = delivery.Courier == null ? null : $"{delivery.Courier.FirstName} {delivery.Courier.LastName}",
                GuestName = order.Guest == null ? null : $"{order.Guest.FirstName} {order.Guest.LastName}",
                Items = items,
                Subtotal = subtotal,
                DeliveryFee = delivery.DeliveryFee,
                Total = subtotal + delivery.DeliveryFee
            };
        }

        public async Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> ChangeStatusAsync(int orderId, DeliveryStatus newStatus)
        {
           await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync (orderId);

            var order = await LoadOnlineOrderAsync (orderId);

            if(order is null)
            {
                return (null, "Order not found or is not an online order.", 404);
            }

            var delivery = order.Delivery!;

            if(!ManagerTransitions.TryGetValue(delivery.Status, out var allowedStatuses) || !allowedStatuses.Contains(newStatus))
            {
                return (null, $"Cannot change status from {delivery.Status} to {newStatus}.", 400);
            }

            if(newStatus == DeliveryStatus.PickedUp && order.Channel  != OrderChannel.Pickup)
            {
                return (null, "Only pickup orders can be marked as PickedUp.", 400);
            }

            var now = _timeProvider.GetLocalNow().DateTime;
            var oldStatus = delivery.Status;
            delivery.Status = newStatus;

            if(newStatus == DeliveryStatus.Ready)
            {
                delivery.ReadyAt = now;

                foreach (var item in order.Items)
                {
                    item.Status = OrderItemStatus.Ready;
                }
            }
            else if(newStatus == DeliveryStatus.PickedUp)
            {
                CompleteWithInvoice(order, now);
            }
            else if(newStatus == DeliveryStatus.Cancelled)
            {
                CancelOrder(order, now);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyAsync(order);

            _logger.LogInformation("Online order {OrderId} moved {OldStatus} -> {NewStatus}", order.Id, oldStatus, newStatus);

            return (ToDto(order), null, 200);
        }

        private async Task NotifyAsync(Order order)
        {
            await _hub.Clients.Groups("managers", "couriers", $"user-{order.GuestId}").SendAsync("OnlineOrdersChanged");
            await _hub.Clients.Group("kitchen").SendAsync("KitchenChanged");
        }
        private async Task LockOrderAsync(int orderId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE Id = {orderId}");
        }

        private async Task<Order?> LoadOnlineOrderAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Guest)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .Include(o => o.Delivery).ThenInclude(d => d!.Courier)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Channel != OrderChannel.DineIn);
        }

        private void CompleteWithInvoice(Order order, DateTime now)
        {
            var delivery = order.Delivery!;
            delivery.CompletedAt = now;

            order.Status = OrderStatus.Closed;
            order.ClosedAt = now;

            _context.Invoices.Add(new Invoice
            {
                OrderId = order.Id,
                IssuedAt = now,
                PaymentMethod = delivery.PaymentMethod,
                TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice) + delivery.DeliveryFee
            });
        }

        private static void CancelOrder(Order order, DateTime now)
        {
            var delivery = order.Delivery!;
            delivery.Status = DeliveryStatus.Cancelled;
            delivery.CompletedAt = now;

            order.Status = OrderStatus.Cancelled;
            order.ClosedAt = now;
        }

        public async Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> CancelMineAsync(int orderId, int guestId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var order = await LoadOnlineOrderAsync(orderId);
            if (order is null || order.GuestId != guestId)
            {
                return (null, "Order not found or does not belong to you.", 404);
            }

            if(order.Delivery!.Status != DeliveryStatus.Received)
            {
                return (null, "Order can be cancelled only until the restaurant confirms it.", 409);
            }

            CancelOrder(order, _timeProvider.GetLocalNow().DateTime);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyAsync(order);

            _logger.LogInformation("Online order {OrderId} cancelled by guest {GuestId}", order.Id, guestId);

            return (ToDto(order), null, 200);
        }

        public async Task<List<OnlineOrderDto>> GetAvailableAsync()
        {
            var orders = await _context.Orders
                .Where(o => o.Channel == OrderChannel.Delivery
                    && o.Status == OrderStatus.Open
                    && o.Delivery!.Status == DeliveryStatus.Ready
                    && o.Delivery.CourierId == null)
                .Include(o => o.Guest)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .Include(o => o.Delivery).ThenInclude(d => d!.Courier)
                .OrderBy(o => o.Delivery!.ReadyAt)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }

        public async Task<List<OnlineOrderDto>> GetCourierActiveAsync(int courierId)
        {
            var orders = await _context.Orders
                .Where(o => o.Delivery!.CourierId == courierId
                    && o.Delivery.Status == DeliveryStatus.OutForDelivery)
                .Include(o => o.Guest)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .Include(o => o.Delivery).ThenInclude(d => d!.Courier)
                .ToListAsync();

            return orders.Select(ToDto).ToList();
        }

        public async Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> ClaimAsync(int orderId, int courierId)
        {
            var now = _timeProvider.GetLocalNow().DateTime;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var hasActive = await _context.Deliveries
               .AnyAsync(d => d.CourierId == courierId && d.Status == DeliveryStatus.OutForDelivery);
            if (hasActive)
            {
                return (null, "You already have a delivery in progress.", 409);
            }

            int rows;
            try
            {
                rows = await _context.Deliveries
                    .Where(d => d.OrderId == orderId
                        && d.Order.Channel == OrderChannel.Delivery
                        && d.Status == DeliveryStatus.Ready
                        && d.CourierId == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(d => d.CourierId, courierId)
                        .SetProperty(d => d.Status, DeliveryStatus.OutForDelivery)
                        .SetProperty(d => d.TakenAt, now));
            }
            catch (Exception ex) when (ex is DbUpdateException || ex is SqlException)
            {
                return (null, "You already have a delivery in progress.", 409);
            }

            if (rows == 0)
            {
                return (null, "This order is no longer available.", 409);
            }

            await transaction.CommitAsync();

            var order = await LoadOnlineOrderAsync(orderId);
            await NotifyAsync(order!);

            _logger.LogInformation("Courier {CourierId} took delivery {OrderId}", courierId, orderId);

            return (ToDto(order!), null, 200);
        }

        public async Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> DeliverAsync(int orderId, int courierId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var order = await LoadOnlineOrderAsync(orderId);
            if (order is null || order.Delivery!.CourierId != courierId)
            {
                return (null, "Order not found or is not assigned to you.", 404);
            }

            var delivery = order.Delivery;
            if (delivery.Status != DeliveryStatus.OutForDelivery)
            {
                return (null, "Order is not out for delivery.", 409);
            }

            delivery.Status = DeliveryStatus.Delivered;
            CompleteWithInvoice(order, _timeProvider.GetLocalNow().DateTime);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyAsync(order);

            _logger.LogInformation("Courier {CourierId} delivered order {OrderId}", courierId, orderId);

            return (ToDto(order), null, 200);
        }
    }
}