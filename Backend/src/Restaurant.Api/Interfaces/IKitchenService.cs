using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface IKitchenService
    {
        Task<List<KitchenTicketDto>> GetKitchenTicketAsync();
        Task<(string? Error, int StatusCode)> ChangeItemStatusAsync(int itemId, OrderItemStatus newStatus);
        Task<(string? Error, int StatusCode)> ChangeOrderStatusAsync(int orderId, OrderItemStatus newStatus);
    }
}
