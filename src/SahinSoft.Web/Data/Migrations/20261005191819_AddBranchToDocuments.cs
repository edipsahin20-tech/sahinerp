using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "DispatchNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "BusinessOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BranchId",
                table: "Invoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchNotes_BranchId",
                table: "DispatchNotes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessOrders_BranchId",
                table: "BusinessOrders",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessOrders_Branches_BranchId",
                table: "BusinessOrders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchNotes_Branches_BranchId",
                table: "DispatchNotes",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Branches_BranchId",
                table: "Invoices",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Mevcut kayıtlar: şube atanmamış belge/hareketler merkez şubeye (yoksa ilk şubeye) bağlanır; "Şube atanmamış" kalmasın.
            // Taslak makbuzlar hariç (hesap-şube uygunluğu onayda denetlenir).
            migrationBuilder.Sql(@"
DECLARE @head int = (SELECT TOP 1 Id FROM Branches ORDER BY IsHeadOffice DESC, Id);
IF @head IS NOT NULL
BEGIN
    UPDATE Invoices SET BranchId = @head WHERE BranchId IS NULL;
    UPDATE DispatchNotes SET BranchId = @head WHERE BranchId IS NULL;
    UPDATE BusinessOrders SET BranchId = @head WHERE BranchId IS NULL;
    UPDATE PaymentReceipts SET OriginBranchId = @head WHERE OriginBranchId IS NULL AND Status <> 1;
    UPDATE CurrentAccountTransactions SET OriginBranchId = @head WHERE OriginBranchId IS NULL;
    UPDATE FinancialTransactions SET OriginBranchId = @head WHERE OriginBranchId IS NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessOrders_Branches_BranchId",
                table: "BusinessOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_DispatchNotes_Branches_BranchId",
                table: "DispatchNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Branches_BranchId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BranchId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_DispatchNotes_BranchId",
                table: "DispatchNotes");

            migrationBuilder.DropIndex(
                name: "IX_BusinessOrders_BranchId",
                table: "BusinessOrders");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "DispatchNotes");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "BusinessOrders");
        }
    }
}
