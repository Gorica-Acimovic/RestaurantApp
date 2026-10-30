using Restaurant.Api.DTOs;
using System.Threading.Tasks;

namespace Restaurant.Api.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> RegisterManagerAsync(RegisterRequest request, int adminId);
        Task<(UserDto? Waiter,string? Error)> RegisterWaiterAsync(RegisterWaiterRequest request, int managerId);

        Task<List<UserDto>> GetAllManagersAsync(int adminId);
        Task<List<UserDto>> GetAllWaitersAsync(int managerId);

        Task <bool> SetManagersProfileStatus(int managerId, int adminId, bool isActive);
        Task<(bool Found, string? Error)> SetWaitersProfileStatus(int waiterId, int managerId, bool isActive);

        Task<(bool Found, string? Error)> UpdateManagerAsync(int managerId, int adminId, UpdateManagerRequest request);
        Task<(bool Found, string? Error)> UpdateWaiterAsync(int waiterId, int managerId, UpdateWaiterRequest request);

        Task<UserDto?> GetProfileAsync(int userId);
        Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, ChangePasswordRequest request);

        Task<(string? Error, int StatusCode)> DeactivateOwnAccountAsync (int userId, DeactivateAccountRequest request); 
    }
}
