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

        public async Task<(MenuItemDto? Item, string? Error)> CreateMenuItemAsync(SaveMenuItemRequest request)
        {
            var subcategory = await _context.MenuSubcategories.FirstOrDefaultAsync(s => s.Id == request.SubcategoryId);
            if (subcategory == null)
            {
                return (null, "Subcategory not found.");
            }

            var menuItem = new MenuItem
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                SubcategoryId = subcategory.Id,
            };
            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            var menuItemDto = new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = subcategory.Category,
                SubcategoryId = subcategory.Id,
                SubcategoryName = subcategory.Name,
                IsAvailable = menuItem.IsAvailable
            };

            return (menuItemDto, null);
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
                query = query.Where(m => m.Subcategory.Category == category.Value);
            }

            var items = await query
                .OrderBy(m => m.Subcategory.Category == MenuCategory.Drink)
                .ThenBy(m => m.Subcategory.SortOrder)
                .ThenBy(m => m.Name)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Category = m.Subcategory.Category,
                    SubcategoryId = m.SubcategoryId,
                    SubcategoryName = m.Subcategory.Name,
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

        public async Task<(bool Found, string? Error)> UpdateMenuItemAsync(int id, SaveMenuItemRequest request)
        {
            var menuItem = await _context.MenuItems.FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            if (menuItem == null)
            {
                return (false, "Menu item not found.");
            }

            var subcategoryExists = await _context.MenuSubcategories.AnyAsync(s => s.Id == request.SubcategoryId);
            if (!subcategoryExists)
            {
                return (true, "Subcategory not found.");
            }

            menuItem.Name = request.Name;
            menuItem.Description = request.Description;
            menuItem.Price = request.Price;
            menuItem.SubcategoryId = request.SubcategoryId;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<MenuSubcategoryDto>> GetSubcategoriesAsync()
        {
            return await _context.MenuSubcategories
                .OrderBy(s => s.Category == MenuCategory.Drink)
                .ThenBy(s => s.SortOrder)
                .Select(s => new MenuSubcategoryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    SortOrder = s.SortOrder
                })
                .ToListAsync();
        }
    }
}
