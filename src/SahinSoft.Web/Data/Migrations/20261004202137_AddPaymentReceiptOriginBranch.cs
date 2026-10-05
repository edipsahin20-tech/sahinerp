using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceiptOriginBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginBranchId",
                table: "PaymentReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_OriginBranchId",
                table: "PaymentReceipts",
                column: "OriginBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Branches_OriginBranchId",
                table: "PaymentReceipts",
                column: "OriginBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Branches_OriginBranchId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_OriginBranchId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "OriginBranchId",
                table: "PaymentReceipts");
        }
    }
}
