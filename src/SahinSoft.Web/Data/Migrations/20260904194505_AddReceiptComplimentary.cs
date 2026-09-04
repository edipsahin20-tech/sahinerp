using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptComplimentary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ComplimentaryAtUtc",
                table: "RestaurantChecks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplimentaryByUserId",
                table: "RestaurantChecks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplimentaryNote",
                table: "RestaurantChecks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplimentaryReasonFor",
                table: "RestaurantChecks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplimentaryReasonWhy",
                table: "RestaurantChecks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplimentaryAtUtc",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "ComplimentaryByUserId",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "ComplimentaryNote",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "ComplimentaryReasonFor",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "ComplimentaryReasonWhy",
                table: "RestaurantChecks");
        }
    }
}
