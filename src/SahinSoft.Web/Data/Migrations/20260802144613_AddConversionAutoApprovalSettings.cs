using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConversionAutoApprovalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DispatchToInvoicePurchaseAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DispatchToInvoiceSalesAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrderToDispatchPurchaseAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrderToDispatchSalesAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrderToInvoicePurchaseAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "OrderToInvoiceSalesAutoApprove",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispatchToInvoicePurchaseAutoApprove",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "DispatchToInvoiceSalesAutoApprove",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "OrderToDispatchPurchaseAutoApprove",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "OrderToDispatchSalesAutoApprove",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "OrderToInvoicePurchaseAutoApprove",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "OrderToInvoiceSalesAutoApprove",
                table: "InventorySettings");
        }
    }
}
