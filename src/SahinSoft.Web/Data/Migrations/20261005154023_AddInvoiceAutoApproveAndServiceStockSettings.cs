using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceAutoApproveAndServiceStockSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PurchaseInvoiceAutoApproveOnSave",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SalesInvoiceAutoApproveOnSave",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "StockMovementForServiceItems",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceAutoApproveOnSave",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceAutoApproveOnSave",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "StockMovementForServiceItems",
                table: "InventorySettings");
        }
    }
}
