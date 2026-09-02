using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestaurantModulePhase2Prereqs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsComplimentary",
                table: "RestaurantOrderLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // RESTAURANT_CHECK sayacı bilinçli olarak HasData ile SABİT bir Id vermiyor — 1000'ler
            // aralığı BARCODE_* sayaçlarının organik IDENTITY büyümesiyle çakışabiliyor (bu
            // kurulumda ilk denemede tam bu şekilde bir PK ihlali yaşandı). Id'yi SQL Server
            // IDENTITY'den almasına izin verilir; IF NOT EXISTS ile hem bu migration'ın tekrar
            // çalıştırılmasına hem de satırın elle önceden eklenmiş olmasına karşı güvenlidir.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [NumberSequences] WHERE [Key] = N'RESTAURANT_CHECK')
BEGIN
    INSERT INTO [NumberSequences] ([Key], [Prefix], [NextNumber], [Padding], [RecordId], [CreatedAtUtc])
    VALUES (N'RESTAURANT_CHECK', N'AD.', 1, 5, NEWID(), SYSUTCDATETIME());
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM [NumberSequences] WHERE [Key] = N'RESTAURANT_CHECK';");

            migrationBuilder.DropColumn(
                name: "IsComplimentary",
                table: "RestaurantOrderLines");
        }
    }
}
