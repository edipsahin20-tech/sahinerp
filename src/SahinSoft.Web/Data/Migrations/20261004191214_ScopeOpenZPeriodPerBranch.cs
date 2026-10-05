using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScopeOpenZPeriodPerBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantZPeriods_BranchId",
                table: "RestaurantZPeriods");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantZPeriods_OneOpen",
                table: "RestaurantZPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_OneOpen",
                table: "RestaurantZPeriods",
                columns: new[] { "BranchId", "Status" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantZPeriods_OneOpen",
                table: "RestaurantZPeriods");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_BranchId",
                table: "RestaurantZPeriods",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_OneOpen",
                table: "RestaurantZPeriods",
                column: "Status",
                unique: true,
                filter: "[Status] = 1");
        }
    }
}
