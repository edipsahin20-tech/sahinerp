using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRestaurantZPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantZPeriodId",
                table: "RetailSales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RestaurantZPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    ReceiptCount = table.Column<int>(type: "int", nullable: false),
                    GrossTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ComplimentaryTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantZPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantZPeriods_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RetailSales_RestaurantZPeriodId",
                table: "RetailSales",
                column: "RestaurantZPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_BranchId",
                table: "RestaurantZPeriods",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_OneOpen",
                table: "RestaurantZPeriods",
                column: "Status",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantZPeriods_RecordId",
                table: "RestaurantZPeriods",
                column: "RecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RetailSales_RestaurantZPeriods_RestaurantZPeriodId",
                table: "RetailSales",
                column: "RestaurantZPeriodId",
                principalTable: "RestaurantZPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Z Dönem Kapatma test talimatı (2026-09-06, Edip ek talimat: "Z numarası günlük
            // sıfırlanmayacak") - eski RestaurantCashShift tabanlı Z mekanizması (Z-000001..
            // Z-000011, CreateDirectZReportAsync) bu yeni tabloyla DEĞİŞTİRİLİYOR; numaralandırma
            // aynı diziden DEVAM etsin diye IDENTITY, en son eski Z numarasının bir fazlasından
            // başlatılır. Eski RestaurantCashShift geçmişi (muhasebe/Vardiya amaçlı) SİLİNMEZ,
            // sadece Z numaralandırması için artık kullanılmaz.
            migrationBuilder.Sql("DBCC CHECKIDENT ('RestaurantZPeriods', RESEED, 11);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RetailSales_RestaurantZPeriods_RestaurantZPeriodId",
                table: "RetailSales");

            migrationBuilder.DropTable(
                name: "RestaurantZPeriods");

            migrationBuilder.DropIndex(
                name: "IX_RetailSales_RestaurantZPeriodId",
                table: "RetailSales");

            migrationBuilder.DropColumn(
                name: "RestaurantZPeriodId",
                table: "RetailSales");
        }
    }
}
