using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInvoiceReceiptNumberSequencesAndDefaultFinancialAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "FinancialAccounts",
                columns: new[] { "Id", "AccountType", "BankName", "BranchName", "Code", "CreatedAtUtc", "CurrencyCode", "Iban", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[] { 1, 1, null, null, "KASA", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "TRY", null, true, "Merkez Kasa", null });

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "SALES_INVOICE", 1L, 5, "SF.", null },
                    { 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "PURCHASE_INVOICE", 1L, 5, "AF.", null },
                    { 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "COLLECTION_RECEIPT", 1L, 5, "TAH.", null },
                    { 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "PAYMENT_RECEIPT", 1L, 5, "TED.", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinancialAccounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
