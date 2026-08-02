using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderDispatchInvoiceConversionLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessOrderLineId",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DispatchNoteLineId",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessOrderLineId",
                table: "DispatchNoteLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InvoicedQuantity",
                table: "DispatchNoteLines",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_BusinessOrderLineId",
                table: "InvoiceLines",
                column: "BusinessOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_DispatchNoteLineId",
                table: "InvoiceLines",
                column: "DispatchNoteLineId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNoteLines_BusinessOrderLineId",
                table: "DispatchNoteLines",
                column: "BusinessOrderLineId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DispatchNoteLines_InvoicedQuantity",
                table: "DispatchNoteLines",
                sql: "[InvoicedQuantity] >= 0 AND [InvoicedQuantity] <= [Quantity]");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchNoteLines_BusinessOrderLines_BusinessOrderLineId",
                table: "DispatchNoteLines",
                column: "BusinessOrderLineId",
                principalTable: "BusinessOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_BusinessOrderLines_BusinessOrderLineId",
                table: "InvoiceLines",
                column: "BusinessOrderLineId",
                principalTable: "BusinessOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_DispatchNoteLines_DispatchNoteLineId",
                table: "InvoiceLines",
                column: "DispatchNoteLineId",
                principalTable: "DispatchNoteLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchNoteLines_BusinessOrderLines_BusinessOrderLineId",
                table: "DispatchNoteLines");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_BusinessOrderLines_BusinessOrderLineId",
                table: "InvoiceLines");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_DispatchNoteLines_DispatchNoteLineId",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_BusinessOrderLineId",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_DispatchNoteLineId",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_DispatchNoteLines_BusinessOrderLineId",
                table: "DispatchNoteLines");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DispatchNoteLines_InvoicedQuantity",
                table: "DispatchNoteLines");

            migrationBuilder.DropColumn(
                name: "BusinessOrderLineId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "DispatchNoteLineId",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "BusinessOrderLineId",
                table: "DispatchNoteLines");

            migrationBuilder.DropColumn(
                name: "InvoicedQuantity",
                table: "DispatchNoteLines");
        }
    }
}
