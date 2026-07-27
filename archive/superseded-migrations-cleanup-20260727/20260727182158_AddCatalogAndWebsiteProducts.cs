using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAndWebsiteProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WebsitePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsExempt = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MinimumStockQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TrackStock = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebsitePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    TaxRateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_TaxRates_TaxRateId",
                        column: x => x.TaxRateId,
                        principalTable: "TaxRates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "Name", "UpdatedAtUtc", "WebsitePath" },
                values: new object[,]
                {
                    { 1, "YAZARKASA", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazar Kasa POS", null, "yazarkasa-pos.html" },
                    { 2, "TERAZI", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Teraziler", null, "teraziler.html" },
                    { 3, "BARKOD", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Barkod Okuyucular", null, "barkod-okuyucular.html" },
                    { 4, "YAZICI", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazıcılar", null, "yazicilar.html" },
                    { 5, "ELTERM", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "El Terminalleri", null, "el-terminali.html" },
                    { 6, "POSPC", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Dokunmatik POS", null, "dokunmatik-pos.html" },
                    { 7, "YAZILIM", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Yazılım ve Entegrasyon", null, "kurumsal-yazilim.html" }
                });

            migrationBuilder.InsertData(
                table: "TaxRates",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "IsExempt", "Name", "Rate", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "KDV10", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %10", 10m, null },
                    { 2, "KDV20", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "KDV %20", 20m, null },
                    { 3, "MUAF", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "KDV Muafiyet", 0m, null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "Brand", "CategoryId", "CreatedAtUtc", "Description", "ImagePath", "IsActive", "MinimumStockQuantity", "Model", "Name", "ProductType", "PurchasePrice", "SalePrice", "StockCode", "StockQuantity", "TaxRateId", "TrackStock", "Unit", "UpdatedAtUtc", "WebsitePath" },
                values: new object[,]
                {
                    { 1, null, "Ingenico", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "IDE280", "Ingenico IDE280", "Donanım", 0m, 0m, "YK-0001", 0m, 1, true, "Adet", null, "yazarkasa-pos.html" },
                    { 2, null, "Ingenico", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Move 5000F", "Ingenico Move 5000F", "Donanım", 0m, 0m, "YK-0002", 0m, 1, true, "Adet", null, "yazarkasa-pos.html" },
                    { 3, null, "PayGo", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SP630PRO ECR", "PAYGO SP630PRO ECR", "Donanım", 0m, 0m, "YK-0003", 0m, 1, true, "Adet", null, "yazarkasa-pos.html" },
                    { 4, null, "Profilo", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "S900", "Profilo S900", "Donanım", 0m, 0m, "YK-0004", 0m, 1, true, "Adet", null, "yazarkasa-pos.html" },
                    { 5, null, "inPOS", 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "m530", "inPOS m530 Mobil POS", "Donanım", 0m, 0m, "YK-0005", 0m, 1, true, "Adet", null, "yazarkasa-pos.html" },
                    { 6, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CL3000", "CAS CL3000 Market Terazisi", "Donanım", 0m, 0m, "TR-0001", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 7, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CL8000", "CAS CL8000 Dokunmatik Terazi", "Donanım", 0m, 0m, "TR-0002", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 8, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CN-1", "CAS CN-1 Sistem Terazisi", "Donanım", 0m, 0m, "TR-0003", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 9, null, "Digi", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SM100P", "Digi SM100P Boyunlu Terazi", "Donanım", 0m, 0m, "TR-0004", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 10, null, "Digi", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SM-120T", "Digi SM-120T Dokunmatik Terazi", "Donanım", 0m, 0m, "TR-0005", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 11, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "ER-JR", "CAS ER-JR Masaüstü Terazi", "Donanım", 0m, 0m, "TR-0006", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 12, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "FW-500", "CAS FW-500 Su Geçirmez Terazi", "Donanım", 0m, 0m, "TR-0007", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 13, null, "CAS", 2, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "PDI", "CAS PDI Ankastre Kasa Terazisi", "Donanım", 0m, 0m, "TR-0008", 0m, 2, true, "Adet", null, "teraziler.html" },
                    { 14, null, "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HRS-28", "Hillpos HRS-28", "Donanım", 0m, 0m, "BO-0001", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 15, null, "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HSC-82", "Hillpos HSC-82", "Donanım", 0m, 0m, "BO-0002", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 16, null, "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HSD-92", "Hillpos HSD-92", "Donanım", 0m, 0m, "BO-0003", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 17, null, "Newland", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TP-13", "Newland TP-13", "Donanım", 0m, 0m, "BO-0004", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 18, null, "Newland", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TP-14", "Newland TP-14", "Donanım", 0m, 0m, "BO-0005", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 19, null, "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HS-6700", "Hillpos HS-6700", "Donanım", 0m, 0m, "BO-0006", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 20, null, "Hillpos", 3, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "VS-6800", "Hillpos VS-6800", "Donanım", 0m, 0m, "BO-0007", 0m, 2, true, "Adet", null, "barkod-okuyucular.html" },
                    { 21, null, "Argox", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "OS-214 Plus", "Argox OS-214 Plus Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0001", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 22, null, "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HDT-400", "Hillpos HDT-400 Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0002", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 23, null, "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HTT-440", "Hillpos HTT-440 Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0003", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 24, null, "TSC", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "TTP-244CE", "TSC TTP-244CE Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0004", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 25, null, "Xprinter", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "XP-470B", "Xprinter XP-470B Barkod Yazıcı", "Donanım", 0m, 0m, "YZ-0005", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 26, null, "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "H380", "Hillpos H380 Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0006", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 27, null, "Hillpos", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Q800", "Hillpos Q800 Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0007", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 28, null, "Bixolon", 4, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "SPP-R310", "Bixolon SPP-R310 Mobil Fiş Yazıcı", "Donanım", 0m, 0m, "YZ-0008", 0m, 2, true, "Adet", null, "yazicilar.html" },
                    { 29, null, "Chainway", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C61", "Chainway C61", "Donanım", 0m, 0m, "ET-0001", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 30, null, "Chainway", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C66", "Chainway C66", "Donanım", 0m, 0m, "ET-0002", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 31, null, "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "C7X", "Hillpos C7X Tablet", "Donanım", 0m, 0m, "ET-0003", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 32, null, "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "CM550X", "Hillpos CM550X", "Donanım", 0m, 0m, "ET-0004", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 33, null, "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT42", "Hillpos HT42", "Donanım", 0m, 0m, "ET-0005", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 34, null, "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT42K", "Hillpos HT42K", "Donanım", 0m, 0m, "ET-0006", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 35, null, "Hillpos", 5, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "HT44", "Hillpos HT44", "Donanım", 0m, 0m, "ET-0007", 0m, 2, true, "Adet", null, "el-terminali.html" },
                    { 36, null, "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Touch Pro 15", "Hillpos Touch Pro 15", "Donanım", 0m, 0m, "PC-0001", 0m, 2, true, "Adet", null, "dokunmatik-pos.html" },
                    { 37, null, "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "All-in-One Dual POS", "Hillpos All-in-One Dual POS", "Donanım", 0m, 0m, "PC-0002", 0m, 2, true, "Adet", null, "dokunmatik-pos.html" },
                    { 38, null, "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Slim Touch 15", "Hillpos Slim Touch 15", "Donanım", 0m, 0m, "PC-0003", 0m, 2, true, "Adet", null, "dokunmatik-pos.html" },
                    { 39, null, "Hillpos", 6, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, "Kiosk POS 21.5", "Hillpos Kiosk POS 21.5", "Donanım", 0m, 0m, "PC-0004", 0m, 2, true, "Adet", null, "dokunmatik-pos.html" },
                    { 40, null, null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Özel ERP & CRM Yazılımları", "Yazılım", 0m, 0m, "YW-0001", 0m, 3, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 41, null, null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Stok ve Depo Yönetimi Yazılımı", "Yazılım", 0m, 0m, "YW-0002", 0m, 3, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 42, null, null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "Fabrika ve Üretim Takibi Yazılımı", "Yazılım", 0m, 0m, "YW-0003", 0m, 3, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 43, null, null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "API ve Donanım Entegrasyonları", "Yazılım", 0m, 0m, "YW-0004", 0m, 3, false, "Adet", null, "kurumsal-yazilim.html" },
                    { 44, null, null, 7, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, 0m, null, "GİB & E-Fatura Çözümleri", "Yazılım", 0m, 0m, "YW-0005", 0m, 3, false, "Adet", null, "kurumsal-yazilim.html" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Code",
                table: "ProductCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StockCode",
                table: "Products",
                column: "StockCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_TaxRateId",
                table: "Products",
                column: "TaxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRates_Code",
                table: "TaxRates",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "TaxRates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
