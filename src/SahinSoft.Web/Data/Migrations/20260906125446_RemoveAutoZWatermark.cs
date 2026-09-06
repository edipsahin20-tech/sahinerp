using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAutoZWatermark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAutoZCheckDateLocal",
                table: "InventorySettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastAutoZCheckDateLocal",
                table: "InventorySettings",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastAutoZCheckDateLocal",
                value: null);
        }
    }
}
