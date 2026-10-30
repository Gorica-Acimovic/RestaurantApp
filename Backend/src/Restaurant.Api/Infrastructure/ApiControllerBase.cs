using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;

namespace Restaurant.Api.Infrastructure
{
    public class ApiControllerBase : ControllerBase
    {
        protected IActionResult Error(int statusCode, string message)
        {
            return StatusCode(statusCode, ApiError.Create(HttpContext, statusCode, message));
        }
    }
}
