using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OnlineOrdersController : ApiControllerBase
    {
        private readonly IOnlineOrderService _onlineOrderService;
        private readonly OnlineOrderOptions _options;

        public OnlineOrdersController(IOnlineOrderService onlineOrderService, IOptions<OnlineOrderOptions> options)
        {
            _onlineOrderService = onlineOrderService;
            _options = options.Value;
        }

        [HttpGet("settings")]
        [Authorize(Roles = "Guest")]
        public IActionResult GetSettings()
        {
            return Ok(new { deliveryFee = _options.DeliveryFee });
        }

        [HttpPost]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> Create(CreateOnlineOrderRequest request)
        {
            var guestId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (order, error, statusCode) = await _onlineOrderService.CreateAsync(request, guestId);
            if (order is null)
                return Error(statusCode, error!);

            return Ok(order);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> GetMine()
        {
            var guestId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _onlineOrderService.GetMineAsync(guestId);
            return Ok(orders);
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Guest")]

        public async Task<IActionResult> CancelMine(int id)
        {
            var guestId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (order, error, statusCode) = await _onlineOrderService.CancelMineAsync(id, guestId);
            if (order is null)
                return Error(statusCode, error!);
            return Ok(order);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Manager")]

        public async Task<IActionResult> GetActive()
        {
            var orders = await _onlineOrderService.GetActiveAsync();
            return Ok(orders);
        }

        [HttpPost("{id}/status")]
        [Authorize(Roles = "Manager")]

        public async Task<IActionResult> ChangeStatus (int id, ChangeOnlineOrderStatusRequest request)
        {
            var (order, error, statusCode) = await _onlineOrderService.ChangeStatusAsync(id, request.Status);
            if (order is null)
                return Error(statusCode, error!);
            return Ok(order);
        }

        [HttpGet("available")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> GetAvailable()
        {
            var orders = await _onlineOrderService.GetAvailableAsync();
            return Ok(orders);
        }

        [HttpGet("my-delivery")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> GetMyDelivery()
        {
            var courierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var orders = await _onlineOrderService.GetCourierActiveAsync(courierId);
            return Ok(orders);
        }

        [HttpPost("{id}/claim")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> Claim(int id)
        {
            var courierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (order, error, statusCode) = await _onlineOrderService.ClaimAsync(id, courierId);
            if (order is null)
                return Error(statusCode, error!);
            return Ok(order);
        }

        [HttpPost("{id}/deliver")]
        [Authorize(Roles = "Courier")]
        public async Task<IActionResult> Deliver(int id)
        {
            var courierId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (order, error, statusCode) = await _onlineOrderService.DeliverAsync(id, courierId);
            if (order is null)
                return Error(statusCode, error!);
            return Ok(order);
        }
    }
}

