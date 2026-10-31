using Restaurant.Api.DTOs;
using Restaurant.Api.Models;

namespace Restaurant.Api.Interfaces
{
  

        public interface IMenuService
        {
            Task<List<MenuItemDto>> GetMenuItemsAsync(string? search, MenuCategory? category);
            Task<List<MenuSubcategoryDto>> GetSubcategoriesAsync();
            Task<(MenuItemDto? Item, string? Error)> CreateMenuItemAsync(SaveMenuItemRequest request, int managerId);
            Task<(bool Found, string? Error)> UpdateMenuItemAsync(int id, SaveMenuItemRequest request);
            Task<bool> SetAvailabilityAsync(int id, bool isAvailable);
            Task<bool> DeleteMenuItemAsync(int id);
        }
   
}
