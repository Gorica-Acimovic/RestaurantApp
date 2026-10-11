using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuSubcategoriesController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuSubcategoriesController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubcategories()
        {
            var subcategories = await _menuService.GetSubcategoriesAsync();
            return Ok(subcategories);
        }
    }
}
