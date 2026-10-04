using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class FilterPinIndexByActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PinHash",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PinHash",
                table: "Users",
                column: "PinHash",
                unique: true,
                filter: "[PinHash] is not null and [IsActive]=1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_PinHash",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PinHash",
                table: "Users",
                column: "PinHash",
                unique: true,
                filter: "[PinHash] IS NOT NULL");
        }
    }
}
