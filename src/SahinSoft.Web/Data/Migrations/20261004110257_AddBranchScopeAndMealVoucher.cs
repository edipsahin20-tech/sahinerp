using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchScopeAndMealVoucher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RetailSales",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "RestaurantChecks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginBranchId",
                table: "FinancialTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_BranchId",
                table: "RetailSales",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_BranchId",
                table: "RestaurantChecks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_OriginBranchId",
                table: "FinancialTransactions",
                column: "OriginBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Branches_OriginBranchId",
                table: "FinancialTransactions",
                column: "OriginBranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantChecks_Branches_BranchId",
                table: "RestaurantChecks",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RetailSales_Branches_BranchId",
                table: "RetailSales",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Branches_OriginBranchId",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantChecks_Branches_BranchId",
                table: "RestaurantChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_RetailSales_Branches_BranchId",
                table: "RetailSales");

            migrationBuilder.DropIndex(
                name: "IX_RetailSales_BranchId",
                table: "RetailSales");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantChecks_BranchId",
                table: "RestaurantChecks");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_OriginBranchId",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RetailSales");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "OriginBranchId",
                table: "FinancialTransactions");
        }
    }
}
