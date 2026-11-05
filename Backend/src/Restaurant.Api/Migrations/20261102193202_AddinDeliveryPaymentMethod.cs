using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddinDeliveryPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "Deliveries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Cash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Deliveries");
        }
    }
}
