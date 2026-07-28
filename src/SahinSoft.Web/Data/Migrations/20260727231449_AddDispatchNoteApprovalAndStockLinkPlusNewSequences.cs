using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDispatchNoteApprovalAndStockLinkPlusNewSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DispatchNoteLineId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAtUtc",
                table: "DispatchNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "DispatchNotes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK_RECEIPT", 1L, 5, "SGF.", null },
                    { 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK_ISSUE", 1L, 5, "SCF.", null },
                    { 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK_COUNT", 1L, 5, "SAY.", null },
                    { 9, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "SALES_DISPATCH", 1L, 5, "SIRS.", null },
                    { 10, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "PURCHASE_DISPATCH", 1L, 5, "AIRS.", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_DispatchNoteLineId",
                table: "StockMovements",
                column: "DispatchNoteLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_DispatchNoteLines_DispatchNoteLineId",
                table: "StockMovements",
                column: "DispatchNoteLineId",
                principalTable: "DispatchNoteLines",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_DispatchNoteLines_DispatchNoteLineId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_DispatchNoteLineId",
                table: "StockMovements");

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "DispatchNoteLineId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "ApprovedAtUtc",
                table: "DispatchNotes");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "DispatchNotes");
        }
    }
}
