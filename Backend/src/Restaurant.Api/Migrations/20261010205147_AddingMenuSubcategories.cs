using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddingMenuSubcategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.CreateTable(
                name: "MenuSubcategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuSubcategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MenuSubcategories",
                columns: new[] { "Id", "Category", "Name", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Food", "Appetizers", 1 },
                    { 2, "Food", "Salads", 2 },
                    { 3, "Food", "Main courses", 3 },
                    { 4, "Food", "Desserts", 4 },
                    { 5, "Drink", "Coffee & tea", 1 },
                    { 6, "Drink", "Soft drinks", 2 },
                    { 7, "Drink", "Beer", 3 },
                    { 8, "Drink", "Wine", 4 },
                    { 9, "Drink", "Cocktails", 5 },
                    { 10, "Drink", "Spirits", 6 }
                });

            migrationBuilder.AddColumn<int>(
               name: "SubcategoryId",
               table: "MenuItems",
               type: "int",
               nullable: false,
               defaultValue: 0);

            migrationBuilder.Sql("UPDATE MenuItems SET SubcategoryId = CASE WHEN Category = 'Food' THEN 3 ELSE 6 END");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_SubcategoryId",
                table: "MenuItems",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuSubcategories_Category_Name",
                table: "MenuSubcategories",
                columns: new[] { "Category", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MenuSubcategories_SubcategoryId",
                table: "MenuItems",
                column: "SubcategoryId",
                principalTable: "MenuSubcategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MenuSubcategories_SubcategoryId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "MenuSubcategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_SubcategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "SubcategoryId",
                table: "MenuItems");
        }
    }
}
