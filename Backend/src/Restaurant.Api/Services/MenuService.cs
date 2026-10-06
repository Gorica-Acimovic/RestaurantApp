using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;

        public MenuService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(SaveMenuItemRequest request)
        {
            var menuItem = new MenuItem
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
            };
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var menuItemDto =  new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                IsAvailable = menuItem.IsAvailable
            };

            return menuItemDto;
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
           var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (menuItem == null)
            {
                return false;
            }

            menuItem.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MenuItemDto>> GetMenuItemsAsync(string? search, MenuCategory? category)
        {
            var query = _context.MenuItems.Where(m => m.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(m => m.Name.Contains(term));
            }

            if (category.HasValue)
            {
                query = query.Where(m => m.Category == category.Value);
            }

            var items = await query
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Category = m.Category,
                    IsAvailable = m.IsAvailable
                })
                .ToListAsync();

            return items;

        }

        public async Task<bool> SetAvailabilityAsync(int id, bool isAvailable)
        {
           var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (menuItem == null)
            {
                return false;
            }

            menuItem.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMenuItemAsync(int id, SaveMenuItemRequest request)
        {
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (menuItem == null)
            {
                return false;
            }

            menuItem.Name = request.Name;
            menuItem.Description = request.Description;
            menuItem.Price = request.Price;
            menuItem.Category = request.Category;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
