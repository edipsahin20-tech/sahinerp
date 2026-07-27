using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingWebsiteProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "Name", "UpdatedAtUtc", "WebsitePath" },
                values: new object[] { 8, "POSEKIP", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "POS Çevre Birimleri", null, "index.html" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "Brand", "CategoryId", "CreatedAtUtc", "Description", "ImagePath", "IsActive", "MinimumStockQuantity", "Model", "Name", "ProductType", "PurchasePrice", "SalePrice", "StockCode", "StockQuantity", "TaxRateId", "TrackStock", "Unit", "UpdatedAtUtc", "WebsitePath" },
                values: new object[,]
                {
                    { 45, null, "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Metal Kasa", "Para Çekmecesi", "Donanım", 0m, 0m, "PE-0001", 0m, 2, true, "Adet", null, "index.html" },
                    { 46, null, "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Fiyat Sorgulama Terminali", "Fiyat Gör Cihazı", "Donanım", 0m, 0m, "PE-0002", 0m, 2, true, "Adet", null, "index.html" },
                    { 47, null, "Genel", 8, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Mobil Fiş Yazıcı", "Mobil Yazıcı", "Donanım", 0m, 0m, "PE-0003", 0m, 2, true, "Adet", null, "index.html" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "ProductCategories",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
