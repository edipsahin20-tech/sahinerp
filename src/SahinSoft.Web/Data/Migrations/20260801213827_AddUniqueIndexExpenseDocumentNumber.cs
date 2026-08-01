using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexExpenseDocumentNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Expenses_DocumentNumber",
                table: "Expenses");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DocumentNumber",
                table: "Expenses",
                column: "DocumentNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Expenses_DocumentNumber",
                table: "Expenses");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_DocumentNumber",
                table: "Expenses",
                column: "DocumentNumber");
        }
    }
}
