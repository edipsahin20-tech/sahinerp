using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedStockTransferSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[] { 11, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK_TRANSFER", 1L, 5, "TRF.", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 11);
        }
    }
}
