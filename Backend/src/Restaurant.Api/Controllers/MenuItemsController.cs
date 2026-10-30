using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class MenuItemsController : ApiControllerBase

    {
        private readonly IMenuService _menuItemService;

        public MenuItemsController(IMenuService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMenuItems([FromQuery] string? search, [FromQuery] MenuCategory? category)
        {
            var menuItems = await _menuItemService.GetMenuItemsAsync(search, category);
            return Ok(menuItems);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateMenuItem(SaveMenuItemRequest request)
        {
            var (menuItem, error) = await _menuItemService.CreateMenuItemAsync(request);
            if (error is not null)
                return Error (400,error);

            return Ok(menuItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateMenuItem(int id, SaveMenuItemRequest request)
        {
            var (found, error) = await _menuItemService.UpdateMenuItemAsync(id, request);
            if (!found)
                return Error(404, "Menu item not found.");

            if (error is not null)
                return Error (400,error);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var deleted = await _menuItemService.DeleteMenuItemAsync(id);
            if (!deleted)
                return Error(404, "Menu item not found.");
            return NoContent();
        }

        [HttpPut("{id}/availability")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SetAvailability(int id, SetAvailabilityRequest request)
        {
            var found = await _menuItemService.SetAvailabilityAsync(id, request.IsAvailable);
            if (!found)
                return Error(404, "Menu item not found.");

            return NoContent();
        }
    }
}
