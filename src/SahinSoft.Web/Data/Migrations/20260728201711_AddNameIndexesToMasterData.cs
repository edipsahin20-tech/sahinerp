using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNameIndexesToMasterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_Name",
                table: "UnitsOfMeasure",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Name",
                table: "TaxRates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_Name",
                table: "PriceLists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Name",
                table: "ExpenseCategories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Name",
                table: "Branches",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitsOfMeasure_Name",
                table: "UnitsOfMeasure");

            migrationBuilder.DropIndex(
                name: "IX_TaxRates_Name",
                table: "TaxRates");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_Name",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_PriceLists_Name",
                table: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_Name",
                table: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_Name",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_Branches_Name",
                table: "Branches");
        }
    }
}
