using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class UpdateTablePositionRequest
    {
        [Range(0, 100, ErrorMessage = "Position X must be between 0 and 100.")]
        public double PositionX { get; set; }

        [Range(0, 100, ErrorMessage = "Position Y must be between 0 and 100.")]
        public double PositionY { get; set; }
    }
}
