using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckHeldFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "HeldAtUtc",
                table: "RestaurantChecks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeldByUserId",
                table: "RestaurantChecks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantChecks_HeldAtUtc",
                table: "RestaurantChecks",
                column: "HeldAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RestaurantChecks_HeldAtUtc",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "HeldAtUtc",
                table: "RestaurantChecks");

            migrationBuilder.DropColumn(
                name: "HeldByUserId",
                table: "RestaurantChecks");
        }
    }
}
