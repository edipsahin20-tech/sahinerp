using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Name" },
                values: new object[] { "KDV0", "KDV %0" });

            migrationBuilder.InsertData(
                table: "TaxRates",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "IsExempt", "Name", "Rate", "UpdatedAtUtc" },
                values: new object[] { 4, "KDV1", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %1", 1m, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "TaxRates",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "Name" },
                values: new object[] { "MUAF", "KDV Muafiyet" });
        }
    }
}
