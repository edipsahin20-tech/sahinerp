using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOpenAccountAndAttachedCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttachedCustomerId",
                table: "RestaurantChecks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_AttachedCustomerId",
                table: "RestaurantChecks",
                column: "AttachedCustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantChecks_Customers_AttachedCustomerId",
                table: "RestaurantChecks",
                column: "AttachedCustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantChecks_Customers_AttachedCustomerId",
                table: "RestaurantChecks");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantChecks_AttachedCustomerId",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "AttachedCustomerId",
                table: "RestaurantChecks");
        }
    }
}
