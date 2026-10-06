using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class MenuItemsController : ControllerBase
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
            var menuItem= await _menuItemService.CreateMenuItemAsync(request);
           
            return Ok(menuItem);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateMenuItem(int id, SaveMenuItemRequest request)
        {
            var menuItem  = await _menuItemService.UpdateMenuItemAsync(id, request);
            if (!menuItem)
                return NotFound(new { message = "Menu item not found." });
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var deleted = await _menuItemService.DeleteMenuItemAsync(id);
            if (!deleted)
                return NotFound(new { message = "Menu item not found." });
            return NoContent();
        }

        [HttpPut("{id}/availability")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SetAvailability(int id, SetAvailabilityRequest request)
        {
            var found = await _menuItemService.SetAvailabilityAsync(id, request.IsAvailable);
            if (!found)
                return NotFound(new { message = "Menu item not found." });

            return NoContent();
        }
    }
}
