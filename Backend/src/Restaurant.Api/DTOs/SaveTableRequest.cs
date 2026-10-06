using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class SaveTableRequest
    {
        [Range(1, 1000, ErrorMessage = "Table number must be between 1 and 1000.")]
        public int Number { get; set; }

        [Range(1, 50, ErrorMessage = "Capacity must be between 1 and 50.")]
        public int Capacity { get; set; }

        [Range(0, 100, ErrorMessage = "Position X must be between 0 and 100.")]
        public double PositionX { get; set; }

        [Range(0, 100, ErrorMessage = "Position Y must be between 0 and 100.")]
        public double PositionY { get; set; }
    }
}
