using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddingPinLoginAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PinLoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    BlockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PinLoginAttempts", x => x.Id);
                    table.CheckConstraint("CK_PinLoginAttempts_FailedCount", "[FailedCount] >= 0 ");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PinLoginAttempts_ClientKey",
                table: "PinLoginAttempts",
                column: "ClientKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PinLoginAttempts");
        }
    }
}
