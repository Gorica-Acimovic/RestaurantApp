using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Api.Migrations
{
    /// <inheritdoc />
    public partial class OnlineOrdersFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_TableId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "WaiterId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TableId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Channel",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DineIn");

            migrationBuilder.AddColumn<int>(
                name: "GuestId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OrderItems",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "OrderItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "New");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_GuestId",
                table: "Orders",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId",
                unique: true,
                filter: "[Status] = 'Open' AND [TableId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Channel",
                table: "Orders",
                sql: "([Channel] = 'DineIn' AND [TableId] IS NOT NULL AND [WaiterId] IS NOT NULL) OR ([Channel] <> 'DineIn' AND [TableId] IS NULL AND [GuestId] IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_GuestId",
                table: "Orders",
                column: "GuestId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_GuestId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_GuestId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TableId",
                table: "Orders");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Channel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "OrderItems");

            migrationBuilder.AlterColumn<int>(
                name: "WaiterId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TableId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId",
                unique: true,
                filter: "[Status] = 'Open'");
        }
    }
}
