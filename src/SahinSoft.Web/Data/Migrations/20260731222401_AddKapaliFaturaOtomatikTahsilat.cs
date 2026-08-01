using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKapaliFaturaOtomatikTahsilat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "PaymentReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosedInvoice",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SettlementPaymentMethod",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_InvoiceId",
                table: "PaymentReceipts",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Invoices_InvoiceId",
                table: "PaymentReceipts",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Invoices_InvoiceId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_InvoiceId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "IsClosedInvoice",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SettlementPaymentMethod",
                table: "Invoices");
        }
    }
}
