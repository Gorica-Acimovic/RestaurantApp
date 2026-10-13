using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface IReservationService
    {
        Task<(ReservationDto? Reservation, string? Error, bool Conflict)> CreateReservationAsync(ReservationRequest request, int? userId);
        Task<List<ReservationDto>> GetReservationsAsync(DateTime? date);
        Task<(ReservationDto? Reservation, string? Error, int StatusCode)> ChangeStatusAsync
            (int id, ChangeReservationStatusRequest request, int userId, bool isStaff);

        Task<List<ReservationDto>> GetMyReservationsAsync(int userId);
    }
}
