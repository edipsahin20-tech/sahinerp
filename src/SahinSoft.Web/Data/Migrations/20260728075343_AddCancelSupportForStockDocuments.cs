using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelSupportForStockDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "StockTransfers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "StockTransfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "StockTransfers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "StockSlips",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "StockSlips",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "StockSlips",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "InventoryCounts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "InventoryCounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "InventoryCounts",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "DispatchNotes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAtUtc",
                table: "DispatchNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "DispatchNotes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "StockTransfers");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "StockSlips");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "StockSlips");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "StockSlips");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "InventoryCounts");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "InventoryCounts");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "InventoryCounts");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "DispatchNotes");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "DispatchNotes");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "DispatchNotes");
        }
    }
}
