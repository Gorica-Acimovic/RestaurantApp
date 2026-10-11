using Restaurant.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Api.DTOs
{
    public class SaveMenuItemRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can have at most 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description can have at most 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 99999999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Subcategory is required.")]
        public int SubcategoryId { get; set; }
    }
}
