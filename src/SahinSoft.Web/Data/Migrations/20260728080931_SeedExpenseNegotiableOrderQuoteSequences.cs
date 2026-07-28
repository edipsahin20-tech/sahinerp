using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedExpenseNegotiableOrderQuoteSequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 12, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "EXPENSE", 1L, 5, "MAS.", null },
                    { 13, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "NEGOTIABLE_CHEQUE", 1L, 5, "CEK.", null },
                    { 14, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "NEGOTIABLE_NOTE", 1L, 5, "SEN.", null },
                    { 15, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "SALES_ORDER", 1L, 5, "SSIP.", null },
                    { 16, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "PURCHASE_ORDER", 1L, 5, "ASIP.", null },
                    { 17, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "QUOTE", 1L, 5, "TEK.", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 17);
        }
    }
}
