using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
    public interface IMenuService
    {
        Task<List<MenuItemDto>> GetMenuItemsAsync(string? search,MenuCategory? category);
        Task<MenuItemDto> CreateMenuItemAsync(SaveMenuItemRequest request);
        Task<bool> UpdateMenuItemAsync(int id, SaveMenuItemRequest request);
        Task<bool> SetAvailabilityAsync(int id, bool isAvailable);
        Task<bool> DeleteMenuItemAsync(int id);
    }
}
