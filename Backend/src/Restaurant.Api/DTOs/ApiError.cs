using System.Text.Json.Serialization;

namespace Restaurant.Api.DTOs
{
    public class ApiError
    {
        public int Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]

        public Dictionary<string, string[]>? Errors { get; set; }

        public static ApiError Create(HttpContext httpContext,
            int status, string message, Dictionary<string, string[]>? errors = null)
        {
            return new ApiError
            {
                Status = status,
                Message = message,
                TraceId = httpContext.TraceIdentifier,
                Errors = errors
            };

        }
    }
}
