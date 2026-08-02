using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeGeneratorNumberSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1001, new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Utc), "BARCODE_EAN13", 1L, 8, "1989", null },
                    { 1002, new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Utc), "BARCODE_EAN8", 1L, 3, "1989", null },
                    { 1003, new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Utc), "BARCODE_ASCII", 1L, 6, "AS", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 1003);
        }
    }
}
