using Microsoft.AspNetCore.Diagnostics;
using Restaurant.Api.DTOs;

namespace Restaurant.Api.Infrastructure
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);

            var error = ApiError.Create(httpContext, StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.");

            httpContext.Response.StatusCode = error.Status;
            await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);

            return true;
        }
    }
}
