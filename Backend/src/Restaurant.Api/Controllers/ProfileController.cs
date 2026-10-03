using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService; 
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var profile = await _userService.GetProfileAsync(userId);
            if (profile is null)
                return NotFound(new { message = "Profile not found." });

            return Ok(profile);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (success, error) = await _userService.ChangePasswordAsync(userId, request);

            if (!success)
                return BadRequest(new { message = error });

            return NoContent();
        }   
    }
}
