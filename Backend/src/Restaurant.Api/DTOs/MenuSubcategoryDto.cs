using Restaurant.Api.Models;

namespace Restaurant.Api.DTOs
{
    public class MenuSubcategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MenuCategory Category { get; set; }
        public int SortOrder { get; set; }
    }
}
