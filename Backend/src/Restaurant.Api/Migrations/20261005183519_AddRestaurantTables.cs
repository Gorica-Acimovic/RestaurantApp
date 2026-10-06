using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RestaurantTables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantTables", x => x.Id);
                    table.CheckConstraint("CK_RestaurantTables_Capacity", "[Capacity] >= 1");
                    table.CheckConstraint("CK_RestaurantTables_Number", "[Number] >= 1");
                    table.CheckConstraint("CK_RestaurantTables_PositionX", "[PositionX] >= 0 AND [PositionX] <= 100");
                    table.CheckConstraint("CK_RestaurantTables_PositionY", "[PositionY] >= 0 AND [PositionY] <= 100");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTables_Number",
                table: "RestaurantTables",
                column: "Number",
                unique: true,
                filter: "[IsActive]=1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestaurantTables");
        }
    }
}
