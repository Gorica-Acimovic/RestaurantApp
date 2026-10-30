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
        Task<(int NoShows, int Cancelled)> CloseExpiredReservationsAsync();

        Task<(List<AvailableTableDto>? Tables, string? Error)> GetAvailableTablesAsync(DateTime startsAt, int guestNumber);
        Task<(AvailableTableDto? Table, string? Error, int StatusCode)> SuggestTableAsync(int id);
    }
}
