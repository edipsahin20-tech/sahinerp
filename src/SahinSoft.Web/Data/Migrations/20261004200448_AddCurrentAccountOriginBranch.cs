using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentAccountOriginBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginBranchId",
                table: "CurrentAccountTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrentAccountTransactions_OriginBranchId",
                table: "CurrentAccountTransactions",
                column: "OriginBranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentAccountTransactions_Branches_OriginBranchId",
                table: "CurrentAccountTransactions",
                column: "OriginBranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CurrentAccountTransactions_Branches_OriginBranchId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CurrentAccountTransactions_OriginBranchId",
                table: "CurrentAccountTransactions");

            migrationBuilder.DropColumn(
                name: "OriginBranchId",
                table: "CurrentAccountTransactions");
        }
    }
}
