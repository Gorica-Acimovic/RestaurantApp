using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ManagersController : ControllerBase
    {
        private readonly IUserService _userService;

        public ManagersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterManager(RegisterRequest request)
        {

            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.RegisterManagerAsync(request, adminId);
            if (result is null)
                return Conflict(new { message = "Email is already in use." });

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllManagers()
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var managers = await _userService.GetAllManagersAsync(adminId);
            return Ok(managers);
        }


        [HttpPut("{managerId}/activate")]
        public async Task<IActionResult> ActivateManagersProfile(int managerId)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.SetManagersProfileStatus(managerId, adminId, true);

            if (!result)
                return NotFound(new { message = "Manager is not found or you don't have permission to activate this manager." });

            return NoContent();
        }

        [HttpPut("{managerId}/deactivate")]
        public async Task<IActionResult> DeactivateManagersProfile(int managerId)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _userService.SetManagersProfileStatus(managerId, adminId, false);

            if (!result)
                return NotFound(new { message = "Manager is not found or you don't have permission to deactivate this manager." });

            return NoContent();
        }

        [HttpPut("{managerId}")]
        public async Task<IActionResult> Update(int managerId, UpdateManagerRequest request)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (found, error) = await _userService.UpdateManagerAsync(managerId, adminId, request);

            if (!found)
                return NotFound(new { message = "Manager not found." });

            if (error is not null)
                return Conflict(new { message = error });

            return NoContent();
        }
    }
}
