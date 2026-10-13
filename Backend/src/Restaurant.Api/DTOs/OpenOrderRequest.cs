using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class OpenOrderRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Table is required.")]
        public int TableId { get; set; }

        [Range(1, 50, ErrorMessage = "Number of guests must be between 1 and 50.")]
        public int NumberOfGuests { get; set; }

    }
}
