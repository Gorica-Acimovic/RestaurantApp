using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Waiter,Manager,Admin")]
    public class OrdersController :  ApiControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> OpenOrder(OpenOrderRequest request)
        {
            var waiterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (order, error, statusCode) = await _orderService.OpenOrderAsync(request, waiterId);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(order);
        }

        [HttpGet("table/{tableId}")]
        public async Task<IActionResult> GetOpenOrderForTable(int tableId)
        {
            var order = await _orderService.GetOpenOrderForTableAsync(tableId);
            if (order == null)
                return Error(404, "No open order for this table.");

            return Ok(order);
        }

        [HttpPost("{id}/items")]
        public async Task<IActionResult> AddItem(int id, AddOrderItemRequest request)
        {
            var (order, error, statusCode) = await _orderService.AddItemAsync(id, request);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}")]
        public async Task<IActionResult> UpdateItem(int id, int itemId, UpdateOrderItemRequest request)
        {
            var (order, error, statusCode) = await _orderService.UpdateItemAsync(id, itemId, request);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(order);
        }

        [HttpDelete("{id}/items/{itemId}")]
        public async Task<IActionResult> RemoveItem(int id, int itemId)
        {
            var (order, error, statusCode) = await _orderService.RemoveItemAsync(id, itemId);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(order);
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> CloseOrder(int id, CloseOrderRequest request)
        {
            var (invoice, error, statusCode) = await _orderService.CloseOrderAsync(id, request);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(invoice);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var (error, statusCode) = await _orderService.CancelOrderAsync(id);
                if (error is not null)
                    return Error(statusCode, error);

            return NoContent();
        }

        [HttpGet("{id}/invoice")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var invoice = await _orderService.GetInvoiceAsync(id);
            if (invoice == null)
                return Error(404, "Invoice not found.");

            return Ok(invoice);
        }

    }
}
