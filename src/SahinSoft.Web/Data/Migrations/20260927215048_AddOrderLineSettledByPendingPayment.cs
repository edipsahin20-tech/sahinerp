using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderLineSettledByPendingPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SettledByPendingPaymentId",
                table: "RestaurantOrderLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantOrderLines_SettledByPendingPaymentId",
                table: "RestaurantOrderLines",
                column: "SettledByPendingPaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantOrderLines_RestaurantCheckPendingPayments_SettledByPendingPaymentId",
                table: "RestaurantOrderLines",
                column: "SettledByPendingPaymentId",
                principalTable: "RestaurantCheckPendingPayments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantOrderLines_RestaurantCheckPendingPayments_SettledByPendingPaymentId",
                table: "RestaurantOrderLines");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantOrderLines_SettledByPendingPaymentId",
                table: "RestaurantOrderLines");

            migrationBuilder.DropColumn(
                name: "SettledByPendingPaymentId",
                table: "RestaurantOrderLines");
        }
    }
}
