using Restaurant.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class ChangeReservationStatusRequest
    {
        public ReservationStatus Status { get; set; }

        [Required]
        public string RowVersion { get; set; } = string.Empty;

        public int? TableId { get; set; }
                                         
    }
}
