using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPeriodLockAndInclTaxPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceInclTax",
                table: "InvoiceLines",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedPeriodUntil",
                table: "CompanySettings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompanySettings",
                keyColumn: "Id",
                keyValue: 1,
                column: "ClosedPeriodUntil",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPriceInclTax",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "ClosedPeriodUntil",
                table: "CompanySettings");
        }
    }
}
