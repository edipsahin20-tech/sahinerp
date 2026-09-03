using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyRevenueTarget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyRevenueTarget",
                table: "InventorySettings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "DailyRevenueTarget",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyRevenueTarget",
                table: "InventorySettings");
        }
    }
}
