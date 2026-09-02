using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantModulePhase3RetailSaleTradeType : Migration
    {
        // NOT: IsComplimentary (RestaurantOrderLines) ve ApiKey (Branches) kolonları BURADA
        // scaffold edilmişti ama zaten kendi migration'larında var (RestaurantModulePhase2Prereqs,
        // AddBranchApiKeyForHybridSync) - elle çıkarıldı, aksi halde sıralı uygulamada "kolon zaten
        // var" hatası verirdi. Bu dosyada sadece gerçekten YENİ olan TradeType kalıyor.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TradeType",
                table: "RetailSales",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            // "Perakende Satışlar Carisi" HasData ile SABİT bir Id vermiyor (bkz.
            // RestaurantModulePhase2Prereqs'teki RESTAURANT_CHECK NumberSequence notu) - Customers
            // tablosunda haftalardır gerçek cari kayıtları birikmiş olabileceğinden sabit bir Id
            // prod'da PK ihlaline yol açardı. Id'yi IDENTITY'den almasına izin verilir; IF NOT
            // EXISTS hem tekrar çalıştırmaya hem elle önceden eklenmiş olmaya karşı güvenlidir.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Customers] WHERE [Code] = N'PERAKENDE-SATIS')
BEGIN
    INSERT INTO [Customers] ([Code], [Name], [AccountType], [IsCustomer], [IsSupplier], [IsActive], [RiskLimit], [CreatedByUserId], [CreatedAtUtc], [RecordId])
    VALUES (N'PERAKENDE-SATIS', N'Perakende Satışlar Carisi', 2, 1, 0, 1, 0, N'SYSTEM', SYSUTCDATETIME(), NEWID());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [Customers] WHERE [Code] = N'PERAKENDE-SATIS';");

            migrationBuilder.DropColumn(
                name: "TradeType",
                table: "RetailSales");
        }
    }
}
