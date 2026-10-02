using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SahinSoft.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class CleanupLegacySelfSaleAndPackageVirtualTables : Migration
    {
        // Edip, 2026-09-29: "Muhasebe bölümünde Masa Tanımlarında Self ve Paket masaları hala
        // var, onları temizle" - MakeRestaurantTableOptionalOnSession migration'ı (bir önceki)
        // Self Satış/Paket'i artık masasız yapıyor AMA geçmişte (2026-09-29 mimari karar öncesi)
        // her Self Satış/Paket işlemi için oluşturulmuş 638 adet SAHTE (tek kullanımlık, gizli)
        // RestaurantTable satırı hâlâ veritabanında duruyordu - RestaurantTableSessions.
        // RestaurantTableId hâlâ onlara işaret ediyordu (Channel/BranchId zaten doğru dolduğu
        // için bu FK artık hiçbir işlevsel amaca hizmet etmiyordu, salt tarihi bir kalıntıydı).
        // Önce bu FK'ları null'a çekip (semantik bilgi Channel/BranchId'de zaten var, kayıp yok),
        // sonra sahte masa+salon satırlarını siliyoruz. Silmeden önce elle doğrulandı: hiçbir
        // RestaurantTableReservation/RestaurantTableSessionMove bu sahte masalara referans
        // vermiyor (0 satır her ikisinde de) - bu yüzden güvenle silinebilir. GERÇEK masa
        // satışlarının geçmişine (RestaurantTableId'leri korunan gerçek RestaurantTable'lara
        // işaret eden oturumlar) HİÇ dokunulmaz.
        //
        // Bu migration, IX_RestaurantTableSessions_OneOpenPerTable index'inin "[RestaurantTableId]
        // IS NOT NULL" filtresini içeren düzeltilmiş haliyle çalışmalı (bkz. bir önceki migration
        // FixOpenSessionIndexAllowMultipleNullTables) - düzeltme olmadan aşağıdaki UPDATE, aynı anda
        // birden fazla NULL RestaurantTableId'yi eski (hatalı) unique index'e yazmaya çalışıp
        // "duplicate key value is (<NULL>)" hatasıyla başarısız olurdu (SQL Server filtreli unique
        // index'lerde NULL'ları birbirinin duplicate'i sayar, ANSI-SQL'in aksine).
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE rts
                SET rts.RestaurantTableId = NULL
                FROM RestaurantTableSessions rts
                JOIN RestaurantTables rt ON rt.Id = rts.RestaurantTableId
                JOIN RestaurantSections rs ON rs.Id = rt.RestaurantSectionId
                WHERE rs.Name IN (N'Self Satış', N'Paket');

                DELETE rt
                FROM RestaurantTables rt
                JOIN RestaurantSections rs ON rs.Id = rt.RestaurantSectionId
                WHERE rs.Name IN (N'Self Satış', N'Paket');

                DELETE FROM RestaurantSections
                WHERE Name IN (N'Self Satış', N'Paket');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alınamaz - silinen sahte masa/salon kayıtları veri kaybı olmadan yeniden
            // üretilemez (zaten hiçbir gerçek bilgi taşımıyorlardı, Channel/BranchId'ye taşındı).
        }
    }
}
