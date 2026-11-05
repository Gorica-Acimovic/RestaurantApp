using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface IOnlineOrderService
    {
        Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> CreateAsync(CreateOnlineOrderRequest request, int guestId);
        Task<List<OnlineOrderDto>> GetMineAsync(int guestId);
        Task<List<OnlineOrderDto>> GetActiveAsync();
        Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> ChangeStatusAsync(int orderId, DeliveryStatus newStatus);
        Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> CancelMineAsync(int orderId, int guestId);
        Task<List<OnlineOrderDto>> GetAvailableAsync(); 
        Task<List<OnlineOrderDto>> GetCourierActiveAsync(int courierId); 
        Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> ClaimAsync(int orderId, int courierId);
        Task<(OnlineOrderDto? Order, string? Error, int StatusCode)> DeliverAsync(int orderId, int courierId);
    }
}
