using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class OnlineOrderItemRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Menu item is required.")]
        public int MenuItemId { get; set; }

        [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99.")]
        public int Quantity { get; set; }

        [StringLength(200, ErrorMessage = "Note can have at most 200 characters.")]
        public string? Note { get; set; }

    }
}
