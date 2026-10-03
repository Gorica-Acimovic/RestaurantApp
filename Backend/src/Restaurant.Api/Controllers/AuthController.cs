using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Services;
using Restaurant.Api.Interfaces;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            if (result is null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _auth.RegisterAsync(request);
            if (result is null)
                return BadRequest(new { message = "Email already exists." });

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest request)
        {
            var result = await _auth.RefreshAsync(request.RefreshToken);
            if (result is null)
                return Unauthorized(new { message = "Refresh token is not valid." });

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequest request)
        {
            await _auth.LogoutAsync(request.RefreshToken);
            return NoContent();
        }

        [HttpPost("pin-login")]

        public async Task<IActionResult> LoginWithPin(PinLoginRequest request)
        {
            var result = await _auth.PinLoginAsync(request);
            if (result is null)
                return Unauthorized(new { message = "Invalid pin." });

            return Ok(result);
        }   
    }
}
