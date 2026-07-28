using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceListsUnitsAndProductFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlternateName",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfOrigin",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PricesIncludeTax",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ShelfLifeDays",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultPriceListId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceLists", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "NumberSequences",
                columns: new[] { "Id", "CreatedAtUtc", "Key", "NextNumber", "Padding", "Prefix", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 18, new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), "PERSONNEL", 1L, 3, "PRSNL.", null },
                    { 19, new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), "CUSTOMER", 1L, 5, "CARI.", null },
                    { 20, new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), "FINANCIAL_ACCOUNT_CASH", 1L, 3, "KASA.", null },
                    { 21, new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), "FINANCIAL_ACCOUNT_BANK", 1L, 3, "BANKA.", null }
                });

            migrationBuilder.InsertData(
                table: "PriceLists",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "MERKEZ", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Merkez Fiyat", null },
                    { 2, "SUBE", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Utc), true, "Şube Fiyat", null }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 23,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 24,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 25,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 33,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 40,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "AlternateName", "CountryOfOrigin", "PricesIncludeTax", "ShelfLifeDays", "UnitOfMeasureId" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.InsertData(
                table: "UnitsOfMeasure",
                columns: new[] { "Id", "Code", "CreatedAtUtc", "DecimalPlaces", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 4, "OZEL", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Özel Fiyat", null },
                    { 5, "METRE", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Metre", null },
                    { 6, "KOLI", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), 0, true, "Koli", null },
                    { 7, "LITRE", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Litre", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DefaultPriceListId",
                table: "AspNetUsers",
                column: "DefaultPriceListId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_Code",
                table: "PriceLists",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceLists_RecordId",
                table: "PriceLists",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_PriceLists_DefaultPriceListId",
                table: "AspNetUsers",
                column: "DefaultPriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_PriceLists_DefaultPriceListId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "PriceLists");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DefaultPriceListId",
                table: "AspNetUsers");

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "NumberSequences",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "UnitsOfMeasure",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "UnitsOfMeasure",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "UnitsOfMeasure",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "UnitsOfMeasure",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "AlternateName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CountryOfOrigin",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PricesIncludeTax",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShelfLifeDays",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultPriceListId",
                table: "AspNetUsers");
        }
    }
}
