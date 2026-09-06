using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoZSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ClosedAutomatically",
                table: "RestaurantZPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoZEnabled",
                table: "InventorySettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "AutoZTimeLocal",
                table: "InventorySettings",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastAutoZCheckDateLocal",
                table: "InventorySettings",
                type: "date",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "InventorySettings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AutoZEnabled", "AutoZTimeLocal", "LastAutoZCheckDateLocal" },
                values: new object[] { false, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedAutomatically",
                table: "RestaurantZPeriods");

            migrationBuilder.DropColumn(
                name: "AutoZEnabled",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "AutoZTimeLocal",
                table: "InventorySettings");

            migrationBuilder.DropColumn(
                name: "LastAutoZCheckDateLocal",
                table: "InventorySettings");
        }
    }
}
