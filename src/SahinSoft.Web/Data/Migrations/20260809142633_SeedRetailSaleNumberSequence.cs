using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedRetailSaleNumberSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RestaurantPostingService.CloseCheckAsync "RETAIL_SALE" anahtarıyla belge numarası
            // üretir ama bu sayaç HİÇ BİR migration'da seed edilmemişti (RESTAURANT_CHECK/
            // PACKAGE_ORDER'ın aksine) - canlı DB'de ilk gerçek adisyon kapanışı denemesinde
            // GenerateWithinTransactionAsync içindeki SingleAsync boş sonuçla patlıyordu
            // ("Sequence contains no elements"). Aynı IF NOT EXISTS deseniyle (bkz.
            // RestaurantModulePhase2Prereqs) düzeltiliyor.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'RETAIL_SALE')
BEGIN
    INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
    VALUES (N'RETAIL_SALE', N'PSF.', 1, 5, NEWID(), SYSUTCDATETIME());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [NumberSequences] WHERE [Key] = N'RETAIL_SALE';");
        }
    }
}
