using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Hubs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System;

namespace Restaurant.Api.Services
{
    public class OrderService : IOrderService
    {

        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly IHubContext<RestaurantHub> _hub;
        private readonly ILogger<OrderService> _logger;


        public OrderService(AppDbContext context, TimeProvider timeProvider, IHubContext<RestaurantHub> hub, ILogger<OrderService> logger)
        {
            _context = context;
            _timeProvider = timeProvider;
            _hub = hub;
            _logger = logger;
        }
        public async Task<(OrderDto? Order, string? Error, int StatusCode)> OpenOrderAsync(OpenOrderRequest request, int waiterId)
        {
            var table = await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == request.TableId && t.IsActive);
            if (table == null)
            {
                return (null, "Table not found.", 404);
            }

            if (request.NumberOfGuests > table.Capacity)
            {
                return (null, $"Table {table.Number} seats at most {table.Capacity} guests.", 400);
            }

            var hasOpenOrder = await _context.Orders.AnyAsync(o => o.TableId == table.Id && o.Status == OrderStatus.Open);
            if (hasOpenOrder)
            {
                _logger.LogWarning("Order not opened, table {TableId} already has an open order", table.Id);
                return (null, "Table already has an open order.", 409);
            }

            var order = new Order
            {
                TableId = table.Id,
                WaiterId = waiterId,
                OpenedAt = _timeProvider.GetLocalNow().DateTime,
                NumberOfGuests = request.NumberOfGuests
            };

            _context.Orders.Add(order);
            table.Status = TableStatus.Occupied;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {

                _logger.LogWarning("Order not opened, table {TableId} already has an open order", table.Id);
                return (null, "Table already has an open order.", 409);
            }

            await _hub.Clients.All.SendAsync("TablesChanged");
            _logger.LogInformation("Order {OrderId} opened on table {TableId} by waiter {WaiterId}, {Guests} guest(s)", 
                order.Id, table.Id, waiterId, request.NumberOfGuests);

            var dto = await LoadOrderDtoAsync(order.Id);
            return (dto, null, 200);
        }
        public async Task<(OrderDto? Order, string? Error, int StatusCode)> AddItemAsync(int orderId, AddOrderItemRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return (null, "Order not found.", 404);
            }

            if (order.Status != OrderStatus.Open)
            {
                return (null, "Order is not open.", 409);
            }

            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == request.MenuItemId && m.IsActive);
            if (menuItem == null)
            {
                return (null, "Menu item not found.", 404);
            }

            if (!menuItem.IsAvailable)
            {
                return (null, "Menu item is not available.", 400);
            }

            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = menuItem.Id,
                Quantity = request.Quantity,
                UnitPrice = menuItem.Price,
                Note = CleanNote(request.Note)
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyOrderChangedAsync(order.TableId);

            return (await LoadOrderDtoAsync(order.Id), null, 200);
        }
        public async Task<OrderDto?> GetOpenOrderForTableAsync(int tableId)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(o => o.TableId == tableId && o.Status == OrderStatus.Open);

            return order == null ? null : ToDto(order);
        }
        private async Task LockOrderAsync(int orderId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT Id FROM Orders WITH (UPDLOCK, ROWLOCK) WHERE Id = {orderId}");
        }

        private Task NotifyOrderChangedAsync(int tableId)
        {
            return _hub.Clients.Group($"table-{tableId}").SendAsync("OrderChanged");
        }

        private static string? CleanNote(string? note)
        {
            return string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        }
        private async Task<OrderDto?> LoadOrderDtoAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Waiter)
                .Include(o => o.Items).ThenInclude(i => i.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : ToDto(order);
        }

        private static OrderDto ToDto(Order order)
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
                    Note = i.Note
                })
                .ToList();

            return new OrderDto
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table.Number,
                WaiterName = $"{order.Waiter.FirstName} {order.Waiter.LastName}",
                OpenedAt = order.OpenedAt,
                ClosedAt = order.ClosedAt,
                Status = order.Status.ToString(),
                NumberOfGuests = order.NumberOfGuests,
                Items = items,
                Total = items.Sum(i => i.LineTotal)
            };
        }
        public async Task<(OrderDto? Order, string? Error, int StatusCode)> UpdateItemAsync(int orderId, int itemId, UpdateOrderItemRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return (null, "Order not found.", 404);
            }

            if (order.Status != OrderStatus.Open)
            {
                return (null, "Order is not open.", 409);
            }

            var item = await _context.OrderItems.FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId);
            if (item == null)
            {
                return (null, "Order item not found.", 404);
            }

            item.Quantity = request.Quantity;
            item.Note = CleanNote(request.Note);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyOrderChangedAsync(order.TableId);

            return (await LoadOrderDtoAsync(order.Id), null, 200);
        }
        public async Task<(OrderDto? Order, string? Error, int StatusCode)> RemoveItemAsync(int orderId, int itemId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            await LockOrderAsync(orderId);

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return (null, "Order not found.", 404);
            }

            if (order.Status != OrderStatus.Open)
            {
                return (null, "Order is not open.", 409);
            }

            var item = await _context.OrderItems.FirstOrDefaultAsync(i => i.Id == itemId && i.OrderId == orderId);
            if (item == null)
            {
                return (null, "Order item not found.", 404);
            }

            _context.OrderItems.Remove(item);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await NotifyOrderChangedAsync(order.TableId);

            return (await LoadOrderDtoAsync(order.Id), null, 200);
        }
    

    public async Task<(InvoiceDto? Invoice, string? Error, int StatusCode)> CloseOrderAsync(int orderId, CloseOrderRequest request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await LockOrderAsync(orderId);

        var order = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.Waiter)
            .Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return (null, "Order not found.", 404);
        }

        if (order.Status != OrderStatus.Open)
        {
            return (null, "Order is not open.", 409);
        }

        if (order.Items.Count == 0)
        {
            return (null, "Cannot close an order without items.", 400);
        }

        var now = _timeProvider.GetLocalNow().DateTime;

        var invoice = new Invoice
        {
            OrderId = order.Id,
            IssuedAt = now,
            PaymentMethod = request.PaymentMethod,
            TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice)
        };

        _context.Invoices.Add(invoice);
        order.Status = OrderStatus.Closed;
        order.ClosedAt = now;
        order.Table.Status = TableStatus.Available;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
                _logger.LogWarning("Order {OrderId} not closed, it already has an invoice", orderId);

                return (null, "Order already has an invoice.", 409);
        }

        await transaction.CommitAsync();
        await _hub.Clients.All.SendAsync("TablesChanged");

            _logger.LogInformation("Order {OrderId} closed, invoice {InvoiceNumber}, total {Total}" +
                "payment {Paymentmethod}", order.Id, invoice.InvoiceNumber, invoice.TotalAmount, invoice.PaymentMethod);


        return (ToInvoiceDto(order, invoice), null, 200);
    }
    public async Task<(string? Error, int StatusCode)> CancelOrderAsync(int orderId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await LockOrderAsync(orderId);

        var order = await _context.Orders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return ("Order not found.", 404);
        }

        if (order.Status != OrderStatus.Open)
        {
            return ("Order is not open.", 409);
        }

        order.Status = OrderStatus.Cancelled;
        order.ClosedAt = _timeProvider.GetLocalNow().DateTime;
        order.Table.Status = TableStatus.Available;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        await _hub.Clients.All.SendAsync("TablesChanged");

        _logger.LogInformation("Order {OrderId} cancelled on table {TableId}", order.Id, order.TableId);

        return (null, 200);
    }

        public async Task<InvoiceDto?> GetInvoiceAsync(int orderId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order).ThenInclude(o => o.Table)
                .Include(i => i.Order).ThenInclude(o => o.Waiter)
                .Include(i => i.Order).ThenInclude(o => o.Items).ThenInclude(it => it.MenuItem)
                .FirstOrDefaultAsync(i => i.OrderId == orderId);

            return invoice == null ? null : ToInvoiceDto(invoice.Order, invoice);
        }
        private static InvoiceDto ToInvoiceDto(Order order, Invoice invoice)
    {
        var orderDto = ToDto(order);

        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceNumberText = $"INV-{invoice.InvoiceNumber:D6}",
            OrderId = order.Id,
            TableNumber = order.Table.Number,
            WaiterName = orderDto.WaiterName,
            IssuedAt = invoice.IssuedAt,
            PaymentMethod = invoice.PaymentMethod.ToString(),
            Items = orderDto.Items,
            TotalAmount = invoice.TotalAmount
        };
    }
  }
    
}
