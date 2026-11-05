using System.ComponentModel.DataAnnotations;
using Restaurant.Api.Models;

namespace Restaurant.Api.DTOs
{
    public class CreateOnlineOrderRequest
    {
        public OrderChannel Channel { get; set; }

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^[0-9+/ -]{6,20}$", ErrorMessage = "Phone number is not valid.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Address can have at most 200 characters.")]
        public string? Address { get; set; }

        [StringLength(200, ErrorMessage = "Note can have at most 200 characters.")]
        public string? Note { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Add at least one item.")]
        [MinLength(1, ErrorMessage = "Add at least one item.")]
        public List<OnlineOrderItemRequest> Items { get; set; } = new();
    }
}
