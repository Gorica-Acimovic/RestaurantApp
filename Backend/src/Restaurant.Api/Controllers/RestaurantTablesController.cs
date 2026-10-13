using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager,Admin,Waiter")]
    public class RestaurantTablesController : ControllerBase
    {
        private readonly ITableService _tableService;

        public RestaurantTablesController(ITableService tableService)
        {
            _tableService = tableService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            var tables = await _tableService.GetAllTablesAsync();
            return Ok(tables);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterNewTable (SaveTableRequest request)
        {
            var (table, error) = await _tableService.CreateTableAsync(request);
            if(error is not null)
            {
                return Conflict(new { message = error });
            }
            return Ok(table);

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _tableService.DeleteTableAsync(id);
            if(!table)
            {
                return NotFound(new { message = "Table not found." });
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, SaveTableRequest request)
        {
            var (found, error) = await _tableService.UpdateTableAsync(id, request);
            if (!found)
                return NotFound(new { message = "Table not found." });

            if (error is not null)
                return Conflict(new { message = error });

            return NoContent();
        }

        [HttpPut("{id}/position")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePosition(int id, UpdateTablePositionRequest request)
        {
            var found = await _tableService.UpdatePositionOfTableAsync(id, request);
            if (!found)
                return NotFound(new { message = "Table not found." });

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTableStatusRequest request)
        {
            var (found, error) = await _tableService.UpdateStatusAsync(id, request);
            if (!found)
                return NotFound(new { message = "Table not found." });

            if (error is not null)
                return Conflict(new { message = error });

            return NoContent();
        }
    }
}
