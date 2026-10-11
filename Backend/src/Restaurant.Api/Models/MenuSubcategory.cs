namespace Restaurant.Api.Models
{
    public class MenuSubcategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MenuCategory Category { get; set; }
        public int SortOrder { get; set; }  

    }
}
