using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetailSaleDocumentNumberPerBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RetailSales_BranchId",
                table: "RetailSales");

            migrationBuilder.DropIndex(
                name: "IX_RetailSales_DocumentNumber",
                table: "RetailSales");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_BranchId_DocumentNumber",
                table: "RetailSales",
                columns: new[] { "BranchId", "DocumentNumber" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RetailSales_BranchId_DocumentNumber",
                table: "RetailSales");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_BranchId",
                table: "RetailSales",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_DocumentNumber",
                table: "RetailSales",
                column: "DocumentNumber",
                unique: true);
        }
    }
}
