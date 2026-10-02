using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixOpenSessionIndexAllowMultipleNullTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantTableSessions_OneOpenPerTable",
                table: "RestaurantTableSessions");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_OneOpenPerTable",
                table: "RestaurantTableSessions",
                column: "RestaurantTableId",
                unique: true,
                filter: "[Status] = 1 AND [RestaurantTableId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantTableSessions_OneOpenPerTable",
                table: "RestaurantTableSessions");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantTableSessions_OneOpenPerTable",
                table: "RestaurantTableSessions",
                column: "RestaurantTableId",
                unique: true,
                filter: "[Status] = 1");
        }
    }
}
