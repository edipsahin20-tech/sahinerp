using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantOrderLineToStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantOrderLineId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_RestaurantOrderLineId",
                table: "StockMovements",
                column: "RestaurantOrderLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_RestaurantOrderLines_RestaurantOrderLineId",
                table: "StockMovements",
                column: "RestaurantOrderLineId",
                principalTable: "RestaurantOrderLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_RestaurantOrderLines_RestaurantOrderLineId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_RestaurantOrderLineId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "RestaurantOrderLineId",
                table: "StockMovements");
        }
    }
}
