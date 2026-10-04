using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Manager")]
    public class WaitersController : ControllerBase
    {
        private readonly IUserService _userService;
        public WaitersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterWaiter(RegisterWaiterRequest request)
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (waiter, error) = await _userService.RegisterWaiterAsync(request, managerId);
            if (waiter is null)
                return Conflict(new { message = error });

            return Ok(waiter);
        }

        [HttpGet]

        public async Task<IActionResult> GetAllWaiters()
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var waiters = await _userService.GetAllWaitersAsync(managerId);
            return Ok(waiters);
        }

        [HttpPut("{waiterId}/activate")]
        public async Task<IActionResult> ActivateWaitersProfile(int waiterId)
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (found, error) = await _userService.SetWaitersProfileStatus(waiterId, managerId, true);

            if (!found)
                return NotFound(new { message = "Waiter is not found or you don't have permission to activate this waiter." });

            if (error is not null)
                return Conflict(new { message = error });

            return NoContent();
        }

        [HttpPut("{waiterId}/deactivate")]
        public async Task<IActionResult> DeactivateWaitersProfile(int waiterId)
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (found, _) = await _userService.SetWaitersProfileStatus(waiterId, managerId, false);

            if (!found)
                return NotFound(new { message = "Waiter is not found or you don't have permission to deactivate this waiter." });

            return NoContent();
        }

        [HttpPut("{waiterId}")]
        public async Task<IActionResult> Update(int waiterId, UpdateWaiterRequest request)
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (found, error) = await _userService.UpdateWaiterAsync(waiterId, managerId, request);

            if (!found)
                return NotFound(new { message = "Waiter not found." });

            if (error is not null)
                return Conflict(new { message = error });

            return NoContent();
        }
    }
}
