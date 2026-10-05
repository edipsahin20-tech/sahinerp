using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialAccountBranchAndShared : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "FinancialAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "FinancialAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "FinancialAccounts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BranchId", "IsShared" },
                values: new object[] { null, false });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_BranchId",
                table: "FinancialAccounts",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialAccounts_Branches_BranchId",
                table: "FinancialAccounts",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialAccounts_Branches_BranchId",
                table: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_FinancialAccounts_BranchId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "FinancialAccounts");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "FinancialAccounts");
        }
    }
}
