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
    [Authorize]
    public class ProfileController : ApiControllerBase
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
                return Error(404, "Profile not found.");

            return Ok(profile);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var (success, error) = await _userService.ChangePasswordAsync(userId, request);

            if (!success)
                return Error (400,error);

            return NoContent();
        }

        [HttpPut("deactivate")]

        public async Task<IActionResult> Deactivate (DeactivateAccountRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var(error,statusCode) = await _userService.DeactivateOwnAccountAsync(userId,request);
           
            if (error is not null)
                return Error(statusCode, error);

            return NoContent();
        }
    }
}
