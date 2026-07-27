using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBarcodeScaleAndInventoryPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventorySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequireBarcode = table.Column<bool>(type: "bit", nullable: false),
                    AutoGenerateBarcode = table.Column<bool>(type: "bit", nullable: false),
                    DefaultBarcodeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultScalePrefix = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    EnforceStockLevel = table.Column<bool>(type: "bit", nullable: false),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false),
                    AllowSaleWhenOutOfStock = table.Column<bool>(type: "bit", nullable: false),
                    EnableMinimumStockWarning = table.Column<bool>(type: "bit", nullable: false),
                    RequireTransferApproval = table.Column<bool>(type: "bit", nullable: false),
                    TrackStockByVariant = table.Column<bool>(type: "bit", nullable: false),
                    AllowSaleBelowCost = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NumberSequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    Padding = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NumberSequences", x => x.Id);
                    table.CheckConstraint("CK_NumberSequences_NextNumber", "[NextNumber] > 0");
                    table.CheckConstraint("CK_NumberSequences_Padding", "[Padding] BETWEEN 1 AND 12");
                });

            migrationBuilder.CreateTable(
                name: "ScaleProductSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementType = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    PluCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    BarcodeContainsPrice = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleProductSettings", x => x.Id);
                    table.CheckConstraint("CK_ScaleProductSettings_PluLength", "LEN([PluCode]) = 5 AND [PluCode] NOT LIKE '%[^0-9]%'");
                    table.CheckConstraint("CK_ScaleProductSettings_Prefix", "[Prefix] IN (N'27', N'28', N'29')");
                    table.ForeignKey(
                        name: "FK_ScaleProductSettings_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "InventorySettings",
                columns: new[] { "Id", "AllowNegativeStock", "AllowSaleBelowCost", "AllowSaleWhenOutOfStock", "AutoGenerateBarcode", "CreatedAtUtc", "DefaultBarcodeType", "DefaultScalePrefix", "EnableMinimumStockWarning", "EnforceStockLevel", "RequireBarcode", "RequireTransferApproval", "TrackStockByVariant", "UpdatedAtUtc" },
                values: new object[] { 1, false, false, false, true, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "EAN13", "27", true, true, true, true, true, null });

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[] { 1, new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), "STOCK", 1L, 3, "SHN.", null });

            migrationBuilder.InsertData(
                table: "ProductBarcodes",
                columns: new[] { "Id", "Barcode", "BarcodeType", "CreatedAtUtc", "IsActive", "IsPrimary", "ProductId", "ProductVariantId", "UnitMultiplier", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "2000000000015", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 1, null, 1m, null },
                    { 2, "2000000000022", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 2, null, 1m, null },
                    { 3, "2000000000039", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 3, null, 1m, null },
                    { 4, "2000000000046", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 4, null, 1m, null },
                    { 5, "2000000000053", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 5, null, 1m, null },
                    { 6, "2000000000060", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 6, null, 1m, null },
                    { 7, "2000000000077", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 7, null, 1m, null },
                    { 8, "2000000000084", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 8, null, 1m, null },
                    { 9, "2000000000091", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 9, null, 1m, null },
                    { 10, "2000000000107", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 10, null, 1m, null },
                    { 11, "2000000000114", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 11, null, 1m, null },
                    { 12, "2000000000121", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 12, null, 1m, null },
                    { 13, "2000000000138", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 13, null, 1m, null },
                    { 14, "2000000000145", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 14, null, 1m, null },
                    { 15, "2000000000152", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 15, null, 1m, null },
                    { 16, "2000000000169", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 16, null, 1m, null },
                    { 17, "2000000000176", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 17, null, 1m, null },
                    { 18, "2000000000183", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 18, null, 1m, null },
                    { 19, "2000000000190", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 19, null, 1m, null },
                    { 20, "2000000000206", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 20, null, 1m, null },
                    { 21, "2000000000213", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 21, null, 1m, null },
                    { 22, "2000000000220", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 22, null, 1m, null },
                    { 23, "2000000000237", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 23, null, 1m, null },
                    { 24, "2000000000244", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 24, null, 1m, null },
                    { 25, "2000000000251", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 25, null, 1m, null },
                    { 26, "2000000000268", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 26, null, 1m, null },
                    { 27, "2000000000275", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 27, null, 1m, null },
                    { 28, "2000000000282", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 28, null, 1m, null },
                    { 29, "2000000000299", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 29, null, 1m, null },
                    { 30, "2000000000305", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 30, null, 1m, null },
                    { 31, "2000000000312", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 31, null, 1m, null },
                    { 32, "2000000000329", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 32, null, 1m, null },
                    { 33, "2000000000336", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 33, null, 1m, null },
                    { 34, "2000000000343", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 34, null, 1m, null },
                    { 35, "2000000000350", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 35, null, 1m, null },
                    { 36, "2000000000367", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 36, null, 1m, null },
                    { 37, "2000000000374", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 37, null, 1m, null },
                    { 38, "2000000000381", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 38, null, 1m, null },
                    { 39, "2000000000398", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 39, null, 1m, null },
                    { 40, "2000000000404", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 40, null, 1m, null },
                    { 41, "2000000000411", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 41, null, 1m, null },
                    { 42, "2000000000428", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 42, null, 1m, null },
                    { 43, "2000000000435", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 43, null, 1m, null },
                    { 44, "2000000000442", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 44, null, 1m, null },
                    { 45, "2000000000459", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 45, null, 1m, null },
                    { 46, "2000000000466", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 46, null, 1m, null },
                    { 47, "2000000000473", "EAN13", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, true, 47, null, 1m, null }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Barcode",
                value: "2000000000015");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Barcode",
                value: "2000000000022");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Barcode",
                value: "2000000000039");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Barcode",
                value: "2000000000046");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Barcode",
                value: "2000000000053");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Barcode",
                value: "2000000000060");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "Barcode",
                value: "2000000000077");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "Barcode",
                value: "2000000000084");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "Barcode",
                value: "2000000000091");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "Barcode",
                value: "2000000000107");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "Barcode",
                value: "2000000000114");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                column: "Barcode",
                value: "2000000000121");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                column: "Barcode",
                value: "2000000000138");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                column: "Barcode",
                value: "2000000000145");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                column: "Barcode",
                value: "2000000000152");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                column: "Barcode",
                value: "2000000000169");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                column: "Barcode",
                value: "2000000000176");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                column: "Barcode",
                value: "2000000000183");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                column: "Barcode",
                value: "2000000000190");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                column: "Barcode",
                value: "2000000000206");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                column: "Barcode",
                value: "2000000000213");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                column: "Barcode",
                value: "2000000000220");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                column: "Barcode",
                value: "2000000000237");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                column: "Barcode",
                value: "2000000000244");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                column: "Barcode",
                value: "2000000000251");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                column: "Barcode",
                value: "2000000000268");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                column: "Barcode",
                value: "2000000000275");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                column: "Barcode",
                value: "2000000000282");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                column: "Barcode",
                value: "2000000000299");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                column: "Barcode",
                value: "2000000000305");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                column: "Barcode",
                value: "2000000000312");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                column: "Barcode",
                value: "2000000000329");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                column: "Barcode",
                value: "2000000000336");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                column: "Barcode",
                value: "2000000000343");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                column: "Barcode",
                value: "2000000000350");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                column: "Barcode",
                value: "2000000000367");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                column: "Barcode",
                value: "2000000000374");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                column: "Barcode",
                value: "2000000000381");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                column: "Barcode",
                value: "2000000000398");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                column: "Barcode",
                value: "2000000000404");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                column: "Barcode",
                value: "2000000000411");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                column: "Barcode",
                value: "2000000000428");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                column: "Barcode",
                value: "2000000000435");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                column: "Barcode",
                value: "2000000000442");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                column: "Barcode",
                value: "2000000000459");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                column: "Barcode",
                value: "2000000000466");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                column: "Barcode",
                value: "2000000000473");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductBarcodes_Length",
                table: "ProductBarcodes",
                sql: "([BarcodeType] = N'EAN13' AND LEN([Barcode]) = 13) OR ([BarcodeType] = N'EAN8' AND LEN([Barcode]) = 8) OR ([BarcodeType] = N'SCALE' AND LEN([Barcode]) = 7) OR [BarcodeType] = N'OTHER'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductBarcodes_Numeric",
                table: "ProductBarcodes",
                sql: "[BarcodeType] = N'OTHER' OR [Barcode] NOT LIKE '%[^0-9]%'");

            migrationBuilder.CreateIndex(
                name: "IX_NumberSequences_Key",
                table: "NumberSequences",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleProductSettings_Prefix_PluCode",
                table: "ScaleProductSettings",
                columns: new[] { "Prefix", "PluCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleProductSettings_ProductId",
                table: "ScaleProductSettings",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventorySettings");

            migrationBuilder.DropTable(
                name: "NumberSequences");

            migrationBuilder.DropTable(
                name: "ScaleProductSettings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductBarcodes_Length",
                table: "ProductBarcodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductBarcodes_Numeric",
                table: "ProductBarcodes");

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "ProductBarcodes",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                column: "Barcode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                column: "Barcode",
                value: null);
        }
    }
}
