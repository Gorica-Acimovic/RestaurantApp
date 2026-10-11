namespace Restaurant.Api.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set;} = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int SubcategoryId { get; set; }
        public MenuSubcategory Subcategory { get; set; } = null!;
    }
}
