# ŞahinSoft FINAL KABUL TESTİ — Plan ve Sonuç Kaydı

Bu dosya oturumlar arası devamlılık içindir (bağlam dolarsa buradan devam edilir). Gizli değer (parola/PIN) YAZILMAZ.

**Başlangıç:** 2026-10-05 · **Kaynak sürüm (test başı):** git `0e9bba5` · **Yetki:** Edip, "rutin işlemlerde onay bekleme, tek final rapor".
**Test ortamı:** Sunucu (SQL Server) üzerinde ayrı `SahinSoftT_*` test veritabanları; uygulama süreçleri localhost portlarında. `SahinSoftDb`/`SahinSoftDb26` DOKUNULMAZ.
Yedek: `~/Desktop/SahinSoft-Yedek-20261005-1725/` (kaynak+git bundle) + sunucuda `*-20261005-164430-teklif-usd-makbuz.bak`.

## Ortamlar
| Kod | Veritabanı | Port | Amaç |
|---|---|---|---|
| T1 | SahinSoftT_Temiz | 5201 | Boş DB'ye sıfırdan kurulum, TEK ŞUBE |
| T2 | SahinSoftT_Guncel | 5202 | Eski şemadan son sürüme güncelleme |
| M  | SahinSoftT_Merkez | 5210 | Merkez (central) süreci |
| A  | SahinSoftT_SubeA | 5211 | Şube A (Merkez Şube) |
| B  | SahinSoftT_SubeB | 5212 | Şube B (Atabulvarı) |

## Durum anahtarı
✅ Başarılı · ❌ Başarısız(açık) · 🔧 Düzeltildi+yeniden test OK · ⛔ Engelli · ➖ Kapsam dışı · ⏳ Yapılmadı

## Modül envanteri (controller'lar)
Cari(Customers) · Stok(Products,Categories,StockSlips,StockTransfers,InventoryCounts,Warehouses,UnitsOfMeasure,TaxRates,PriceLists) · Fatura(Invoices,DispatchNotes) · Teklif(Quotes) · Sipariş(BusinessOrders) · Masraf(Expenses,ExpenseCategories) · Çek/Senet(NegotiableInstruments) · Kasa/Banka(FinancialAccounts,PaymentReceipts) · Raporlar(Reports,ProductSalesReport,ZPeriods,RetailSales,RetailSaleCancellations) · Sistem Yönetimi(Settings,Roles,Branches,NumberSequences,Printers,PrintTemplates,Personnel) · Restoran(Restaurant,Self/Package,Kitchen,Shift,Reports,DataHealth,TerminalSettings,CashRegisters,Sections,Tables,PermissionProfiles,Auth,Dashboard) · Sync(Api)

## Sonuçlar
(aşağıya her faz bitince eklenir)

## Plana eklenen geliştirme / bulgular (kullanıcı talepleri, 2026-10-05)
- **[GELİŞTİRME-1] Sipariş (alış/satış) ve İrsaliye (alış/satış) ekranlarında alış faturasındaki gibi USD/döviz mantığı:** para birimi + manuel kur, satırda döviz birim fiyat + TL karşılığı, döviz ve TL toplam; sipariş→irsaliye→fatura dönüşümünde döviz fiyat/kur taşınması. Test aşamasında uygulanacak (durum: ⏳).
- **[GELİŞTİRME-2] Fatura satırı:** "Fiyatlara KDV Dahil" işaretliyken ek "Birim Fiyat (KDV Hariç)" sütunu + başlık "Birim Fiyat (KDV Dahil)"; yeni satır eklenince kolonların kayması düzeltildi (tablo sınıfı tabanlı sütun gizleme, sabit tablo yerleşimi). Durum: ✅ uygulandı, ekranda doğrulandı.
- **[BULGU-SahinSoftDb26] "Satış yaptım stok düşmedi":** SHN.008 Market Otomasyon Sistemi ürün tipi Hizmet, stok takibi kapalı → satışta stok hareketi oluşmaması TASARIM gereği. Alış faturası AF.00001 hâlâ Taslak (onaylanmadığı için stok girişi yok). Stok Hareket Ekstresi'ne "stok takibi kapalı" uyarısı eklendi.
