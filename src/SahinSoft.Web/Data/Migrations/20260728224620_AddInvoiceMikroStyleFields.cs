using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceMikroStyleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturn",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerm",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "Invoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalespersonUserId",
                table: "Invoices",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SettlementFinancialAccountId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TradeType",
                table: "Invoices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SettlementFinancialAccountId",
                table: "Invoices",
                column: "SettlementFinancialAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_FinancialAccounts_SettlementFinancialAccountId",
                table: "Invoices",
                column: "SettlementFinancialAccountId",
                principalTable: "FinancialAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_FinancialAccounts_SettlementFinancialAccountId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_SettlementFinancialAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsReturn",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentTerm",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SalespersonUserId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SettlementFinancialAccountId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TradeType",
                table: "Invoices");
        }
    }
}
