using Restaurant.Api.DTOs;

namespace Restaurant.Api.Interfaces
{
    public interface IOrderService
    {
        Task<(OrderDto? Order, string? Error, int StatusCode)> OpenOrderAsync(OpenOrderRequest request, int waiterId);
        Task<OrderDto?> GetOpenOrderForTableAsync(int tableId);
        Task<(OrderDto? Order, string? Error, int StatusCode)> AddItemAsync(int orderId, AddOrderItemRequest request);
        Task<(OrderDto? Order, string? Error, int StatusCode)> UpdateItemAsync(int orderId, int itemId, UpdateOrderItemRequest request);
        Task<(OrderDto? Order, string? Error, int StatusCode)> RemoveItemAsync(int orderId, int itemId);
        Task<(InvoiceDto? Invoice, string? Error, int StatusCode)> CloseOrderAsync(int orderId, CloseOrderRequest request);
        Task<(string? Error, int StatusCode)> CancelOrderAsync(int orderId);
        Task<InvoiceDto?> GetInvoiceAsync(int orderId);
    }
}
