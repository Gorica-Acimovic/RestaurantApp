using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Kitchen, Manager")]
    public class KitchenController : ApiControllerBase
    {
        private readonly IKitchenService _kitchenService;

        public KitchenController(IKitchenService kitchenService)
        {
            _kitchenService = kitchenService;
        }



        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets()
        {
            var tickets = await _kitchenService.GetKitchenTicketAsync();
            return Ok(tickets);
        }

        [HttpPost("items/{itemId}/status")]
        public async Task<IActionResult> ChangeItemStatus(int itemId, ChangeKitchenStatusRequest request)
        {
            var (error, statusCode) = await _kitchenService.ChangeItemStatusAsync(itemId, request.Status);
            if (error is not null)
                return Error(statusCode, error);

            return NoContent();
        }

        [HttpPost("orders/{orderId}/status")]
        public async Task<IActionResult> ChangeOrderStatus(int orderId, ChangeKitchenStatusRequest request)
        {
            var (error, statusCode) = await _kitchenService.ChangeOrderStatusAsync(orderId, request.Status);
            if (error is not null)
                return Error(statusCode, error);

            return NoContent();
        }
    }
}
