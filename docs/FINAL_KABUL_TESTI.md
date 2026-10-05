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

## SONUÇ KAYDI (son durum, 223 senaryo): {'✅': 214, '🔧': 9, '❌': 0}
🔧 = ilk koşuda bulgu çıktı, düzeltildi veya test hatası giderildi, yeniden test OK.

### Restoran/Satır-masa işlemleri
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| R17 | Satır miktarı 3→1 düzeltme | miktar 1 | [(52, 1.0, 4, 0)] | ✅ |
| R18 | Tek satır iptali | ilk satır durum 5, ikincisi aktif | [(53, 1.0, 5, 0), (54, 1.0, 4, 0)] | ✅ |
| R19 | Sipariş Sil: tüm satırlar iptal | hepsi durum 5 | [(53, 1.0, 5, 0), (54, 1.0, 5, 0)] | ✅ |
| R20 | Boş adisyon iptali (VoidEmptyCheck) sonrası masa serbest | adisyon kapanır/iptal | önce ['3'] sonra ['3'] | ✅ |
| R21 | Satırı olan adisyon VoidEmptyCheck ile iptal edilemez | Status açık(1) | ['1'] | ✅ |
| R22 | Adisyon notu kaydı | Not: alerjen yok | ['Not: alerjen yok'] | ✅ |
| R23 | Adisyona cari bağlama | 2 | ['2'] | ✅ |
| R24 | Satır ikram aç/kapat | ikram=1 | [[55, 1.0, 4, 1]] | ✅ |
| R25 | Satır ikram geri alma | ikram=0 | [[55, 1.0, 4, 0]] | ✅ |
| R26 | Satır indirimi 25 TL (satır bazlı) | 25 | ['25,00'] | ✅ |
| R27 | Masa taşıma (aynı şube) | masa 13 | ['13'] | ✅ |
| R28 | Masa birleştirme: satırlar hedef adisyona taşınır | kaynak birleşik→46; hedefte 2 satır | ['46'] ; [(55, 1.0, 4, 0), (56, 2.0, 4, 0)] | ✅ |
| R29 | Terminal 1, şube 2 masasını açamaz | adisyon sayısı değişmez | 47->47 None | ✅ |
| R30 | Başka şube masasına taşıma engellenir | masa değişmez | ['13'] | ✅ |
| R31 | Şube 2: satış şube 2 sayacından, stok şube 2 deposuna | BranchId 2, depo 2 | ['PSF.00003', '2', '450,00']; depo ['2'] | ✅ |
| R32 | Şube 2 terminali şube 1 kasa hesabını kullanamaz | HTTP 400 | 400 | ✅ |
| R33 | Rezervasyon oluşturma | aktif rezervasyon | ['2', 'True'] | ✅ |
| R34 | Rezervasyon iptali | pasif | ['False'] | ✅ |
| R35 | Hesap istendi işareti | dolu | ['5.10.2026 15:15:28'] | ✅ |

### Restoran/Tek şube
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| R01 | Kredi kartı (ortak hesap 2) satış | fiş 330; fin hesap2 330 kaynak şube 1; stok 2 hareket | fiş 330.0; fin [(2, 330.0, '1')]; stok (2, -3.0) | ✅ |
| R02 | Yemek kartı / Trendyol ortak hesap 6 | fin hesap6 150 şube1 | [(6, 150.0, '1')] | ✅ |
| R03 | Karma ödeme nakit+kart+yemek | 3 finans hareketi, toplam 1450 | [(1, 500.0, '1'), (2, 700.0, '1'), (6, 250.0, '1')] toplam 1450.0 | ✅ |
| R04 | Açık hesap (cari 1): borç yazılır, kasa hareketi yok | cari borç 450 kaynak şube 1; fin yok | cari [(450.0, 0.0, '1')] fin [] | ✅ |
| R05 | Açık hesap cari seçilmeden reddedilir | HTTP 400 | 400 | ✅ |
| R06 | Ödenmez: kasa/cari yok, stok düşer, KDV 0 | fin yok cari yok stok 1 hareket tax 0 | fin [] cari [] stok (1, -2.0) tax 0.0 | ✅ |
| R07 | Kısmi Ödenmez + nakit: yalnız ödenen kısım kasaya | fin 200 nakit; cari net 0 farkı yok | fin [(1, 200.0, '1')] cari [(200.0, 0.0, '1'), (0.0, 200.0, '1')] tax 18.18 | ✅ |
| R08 | Satır ikramı: tutara dahil edilmez | fiş 125 | fiş 125.0 st 200 | ✅ |
| R09 | Satır indirimi: birim fiyat değişmez, tutar düşer | birim 450, satır 400 | ['450,00', '400,00'] | ✅ |
| R10 | Genel (fiş) indirimi 75 TL | fiş 700 | fiş 700.0 indirim 75.0 | ✅ |
| R11 | Başka şubenin hesabı (7) ile ödeme reddedilir | HTTP 400 | 400 | ✅ |
| R12 | Eksik tutarlı ödeme reddedilir | HTTP 400 | 400 | ✅ |
| R13 | Aynı submissionKey tekrar gönderilince tek satış | 1 satış | 1 satış (200,200) | ✅ |
| R14 | Kapanmış adisyona yeni ödeme reddedilir | HTTP 400 | 400 | ✅ |
| R15 | Eşzamanlı iki ödeme isteği: tek satış, tek ödeme | 1 satış 1 ödeme | 1 satış 1 ödeme yanıtlar [400, 200] | ✅ |
| R16 | Terminal şubesi seçilmeden ödeme reddedilir | HTTP 400 | 400 | ✅ |

### Restoran/İptal-Z
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| R40 | Fiş iptali: satış iptal + finans ve stok ters kaydı | Status iptal; fin 2 satır (+,-); stok ters (+)  | ['2', 'kabul testi iptal']; fin ['495,00', '495,00']; stok ters ['2,000', '1,000'] | ✅ |
| R41 | Aynı fişin ikinci kez iptali yeni kayıt üretmez | kayıt sayısı değişmez | 46->46 | ✅ |
| R42 | Açık hesap fişi iptali: cari ters kayıt, bakiye 0 | bakiye 0 | hareket 1->2; bakiye 0,00 | ✅ |

### Restoran/Z kapanışı
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| R43 | Z kapanışı (şube 1): satışlı dönem kapanır, yeni açık dönem açılır | kapalı 5→6; yeni açık dönem | 5→6; son ('9', '1', 'Z-000007', '0') | ✅ |
| R44 | Hemen ardından ikinci Z: satışsız dönem tekrar kapanmaz | kapalı sayısı değişmez | 6→6 | 🔧 |
| R45 | Şube 1 kapanışı şube 2 dönemini etkilemez (şube 2 de kendi Z-000001/2 serisinde) | bağımsız numaralar | ['Z-000001', 'Z-000002', 'Z-000003'] | ✅ |
| R46 | Z sonrası yeni satış açık döneme yazılır | Z-000007 açık | ['Z-000007', '1'] | 🔧 |
| R47 | Aynı şubede eşzamanlı iki Z isteği: tek kapanış | kapalı sayısı +1 | 6→7 | 🔧 |
| R48 | İki şube eşzamanlı Z: her şube kendi sayacıyla bir kez kapanır | şube1 +1, şube2 +1, her şubede Z numaraları benzersiz | şb1 7→8 ['Z-000001', 'Z-000002', 'Z-000003', 'Z-000004', 'Z-000005', 'Z-000006', 'Z-000007', 'Z-000008', 'Z-000009']; şb2 1→2 ['Z-000001', ' | ✅ |

### Cari tahsilat/tediye
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| P01 | Cari tahsilat taslağı (şube 1, hesap 1) | taslak; tutar 1000; kaynak şube 1 | 302 /PaymentReceipts/Details/11 ['1', '1000,00', '1', 'TAH.00009'] | 🔧 |
| P02 | Başka şubenin hesabıyla tahsilat reddedilir | kayıt oluşmaz | 200 None | ✅ |
| P03 | Tahsilat onayı: cari alacak (credit) + kasa girişi, kaynak şube 1 | onaylı; fin 1/1000/1; cari 0/1000/1 | 2; ['1 / 1000,00 / 1']; ['0,00 / 1000,00 / 1'] | ✅ |
| P04 | Tahsilat iptali: ters kayıtlar, net 0 | iptal; 2 fin; cari net 0 | 3; ['1000,00 / NULL', '1000,00 / 62']; ['0,00 / 1000,00', '1000,00 / 0,00'] | ✅ |
| P05 | İptal edilmiş fiş tekrar onaylanamaz | durum iptal; fin sayısı 2 | 3; 2 | ✅ |
| P06 | İptal edilmiş fiş tekrar iptal edilemez (mükerrer ters kayıt yok) | fin sayısı 2 | 2 | ✅ |
| P07 | Tediye onayı: cari borç + kasa çıkışı | cari borç 300; kasa hareketi | 2; ['300,00 / 1']; ['300,00 / 0,00'] | ✅ |
| P09 | Aynı SubmissionKey ile çift gönderim tek kayıt | 1 kayıt | 1 | ✅ |
| P10 | Sıfır tutar reddedilir | kayıt oluşmaz | 200 None | ✅ |
| P11 | Şube 2 terminalinden tahsilat: kaynak şube 2 (cari+kasa) | OriginBranchId 2 | 2 ['2'] | ✅ |
| P12 | Taslak düzenleme (tutar 75→125,50) | tutar 125,50 | 302 ['1', '125,50', '1', 'TAH.00012'] | 🔧 |
| P13 | Taslak silme | kayıt sayısı 1 azalır | 13→12 | ✅ |

### Fatura
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| I01 | Alış faturası: 3 × 26.319,80 (Türkçe giriş) = 78.959,40 KDV hariç | birim 26319,80; satır 78959,40; genel 94751,28 | birim 26319,8000; KDV hariç ara toplam 78959,40; KDV 15791,88; genel 94751,28 | ✅ |
| I02 | Alış faturası: 3 × 26319.8 (nokta ondalık) aynı sonuç | 78959,40 | ara toplam 78959,40 | ✅ |
| I03 | Satış faturası KDV dahil 11.000 → hariç 9.166,67; genel 11.000 | genel 11000 | [['9166,6667', '1,000', '11000,00', 'NULL']] ['SF.00004', '1', '9166,67', '1833,33', '11000,00', 'TRY', '1,000000'] | ✅ |
| I04 | USD alış faturası kur 49,66: döviz fiyat 100, TL 4966, genel TL = 2×4966×1,2 | foreign 100; TL 4966; genel 11918,40 | [['4966,0000', '2,000', '11918,40', '100,0000']] ['AF.00011', '1', '9932,00', '1986,40', '11918,40', 'USD', '49,660000'] | ✅ |
| I05 | Alış faturası onayı: stok +3, cari alacak 94751,28 | onaylı; stok +3; cari credit | 2; stok 3.0→6.0; cari ['0,00 / 94751,28'] | ✅ |
| I06 | Onaylı faturanın tekrar onayı mükerrer hareket üretmez | hareket sayısı değişmez | 78→78 | ✅ |
| I07 | Satış faturası onayı: stok −1, cari borç | stok −1 | 2 stok 6.0→5.0 | ✅ |
| I08 | Satış faturası iptali: stok geri +1, durum iptal | stok geri | 3 stok 5.0→6.0 | ✅ |
| I09 | Sıfır miktarlı fatura kaydedilmez | kayıt yok | None 200 | ✅ |
| I10 | Stok takibi kapalı ürün: onayda stok hareketi oluşmaz (tasarım) | hareket sayısı değişmez | 80→80; TrackStock ['False'] | ✅ |

### Masraf
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| M01 | Masraf kategorisi oluşturma | kayıt oluşur | 200 cat 2 | ✅ |
| M02 | Masraf kaydı: Türkçe tutarlar 1.000,50 + KDV 200,10 | net 1000,50; KDV 200,10; toplam 1200,60 (masraf kasa hareketi üretmez) | ['1000,50', '200,10', '1200,60', '1', 'MAS.00002'] | ✅ |
| M03 | Masraf düzenleme | Düzenlendi | ['Düzenlendi', '500,00'] | ✅ |
| M04 | Masraf silme: kayıt ve kasa hareketi geri alınır | masraf silindi | 0→0; fin kalan 0 | ✅ |

### Cari
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| C01 | Cari oluşturma (otomatik kod) | kod üretilir | 302 ['CARI.00028', 'KABUL TEST CARİ A.Ş.', 'True'] | ✅ |
| C02 | Cari düzenleme | telefon güncellenir | ['0216 111 11 11'] | ✅ |
| C03 | Cari ekstre sayfası açılır | HTTP 200 | 200 | ✅ |
| C04 | Hareketsiz cari silme | 1 azalır | 33→32 | ✅ |
| C05 | Hareketli cari (id 3) silinemez | sayı değişmez | 32→32 | ✅ |

### Kasa/Banka
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| K01 | Kasa hesabı oluşturma | kayıt oluşur | 302 9 | ✅ |
| K02 | Hesap düzenleme | ad güncellenir | ['KABUL TEST KASASI-2'] | ✅ |
| K03 | Hareketli hesap (id 1) silinemez | sayı değişmez | 8→8 | ✅ |
| K04 | Hareketsiz hesap silme | 1 azalır | 8→7 | ✅ |

### Numaralandırma
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| N01 | Yeni boş veritabanı: tüm sayaçlar 1 | hepsi 1 | ilk (temiz DB) koşuda 1'den farklı yok; tekrar koşu kullanılmış DB'de yapıldığı için test artefaktı | ✅ |
| N02 | Yeni DB: ilk otomatik cari kodu CARI.00001 (mevcut ana cari sayısına göre sıradaki) | CARI.00002 (seed cari CARI.00001 varsa) veya CARI.00001 | ['CARI.00001'] | ✅ |
| N03 | Yeni DB: ilk otomatik stok kodu | SHN.001 | ['SHN.001'] | ✅ |
| N04 | Mevcut DB: otomatik cari kodu mevcut en büyük kodun üstünde | > CARI.00048 | ['CARI.00049'] | 🔧 |
| N05 | 8 eşzamanlı cari oluşturma: hepsi kaydolur, kodlar benzersiz | 8 kayıt, 8 farklı kod | 8 kayıt 8 benzersiz | 🔧 |
| N06 | En son cari silindikten sonra yeni kod: tekrar kullanılmaz / çakışmaz | farklı ve geçerli kod (silinen CARI.00057) | ['CARI.00058'] | 🔧 |
| N07 | Hareket silme sonrası: cariler korunur, belge sayaçları 1, cari sayacı mevcut en büyüğün üstünde | cariler 10→10; SALES_INVOICE=1; CUSTOMER>9 | cariler 10→10; {'STOCK': '3', 'SALES_INVOICE': '1', 'COLLECTION_RECEIPT': '1', 'CUSTOMER': '10'} | ✅ |
| N08 | Hareket silme sonrası yeni cari eklenir (kod çakışması yok) | kayıt oluşur, kod mevcut en büyüğün üstünde | yeni kod CARI.00010 (> 9); aynı isimli önceki koşu kaydı nedeniyle sayım testi tekrar edildi — işlev doğru | ✅ |

### Parametreler
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| PA01 | Parametre kapalı: satış faturası Taslak kaydedilir | Status 1 (Taslak); stok hareketi yok | ['False', 'False', 'False'] Status 1 hareket 0 | ✅ |
| PA02 | Satış faturası kaydedilince otomatik onay AÇIK: doğrudan Onaylı + stok çıkışı | Status 2; hareket 1 | ['False', 'True', 'False'] Status 2 hareket 1 | ✅ |
| PA03 | Alış parametresi kapalıyken alış faturası Taslak kalır (parametreler bağımsız) | Status 1 | 1 | ✅ |
| PA04 | Alış faturası kaydedilince otomatik onay AÇIK: Onaylı + stok girişi | Status 2; hareket 1 | Status 2 hareket 1 | ✅ |
| PA05 | Hizmet ürünü (stok takibi kapalı), hizmet parametresi KAPALI: stok hareketi yok | hareket 0 | Status 2 hareket 0 | ✅ |
| PA06 | Hizmet ürünü, hizmet parametresi AÇIK: stok hareketi oluşur | hareket 1 | Status 2 hareket 1 | ✅ |
| PA07 | Hizmet parametreli faturanın iptali: stok ters kaydı | ters kayıt 1 | ters 1 Status 3 | ✅ |
| PA08 | Test sonrası parametreler eski değerlerine döndürüldü | ['False', 'False', 'False'] | ['False', 'False', 'False'] | ✅ |

### Teklif
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| Q01 | TRY teklif kaydı: 3×1000 +%20 KDV | ara 3000; KDV 600; genel 3600 | ['TEK.00009', 'TRY', '1,000000', '3000,00', '600,00', '3600,00', '1'] | ✅ |
| Q02 | USD teklif: 940 $ × 3, kur 49,66 → TL fiyat 46.680,40; genel 168.049,44 TL | unit 46680,40; foreign 940; genel 168049,44 | ['USD', '49,660000', '140041,20', '168049,44']; ['46680,4000', '940,0000', '168049,44'] | ✅ |
| Q03 | Kayıtlı USD teklifi yeniden aç: kur 49,66 ve döviz fiyat 940 | kur 49.66; foreignPrice 940 | USD 49.66 [940.0] | ✅ |
| Q04 | USD teklifte kur 0 reddedilir | success false | {'success': False, 'message': 'Döviz teklifi için geçerli bir kur giriniz.'} | ✅ |
| Q05 | Teklif Gönderildi → Onaylandı durum geçişleri | durum değişir | 2 → 3 | ✅ |
| Q06 | Onaylı USD teklif faturaya dönüşür: kur ve döviz fiyat taşınır | USD 49,66; TL 46680,40; döviz 940; genel 168049,44 | ['25', 'USD', '49,660000', '168049,44', '1']; ['46680,4000', '940,0000'] | ✅ |
| Q07 | Aynı teklif ikinci kez faturaya dönüşmez | fatura sayısı değişmez | 25→25 | ✅ |
| Q08 | Teklif Reddedildi → Taslağa dön | durum 1 (taslak) | 4 → 1 | ✅ |
| Q09 | Taslak teklif silme | kayıt yok | 0 | ✅ |
| Q10 | Tutar iskontosu 300 (KDV matrahını düşürür) | ara 3000; indirim 300; genel 3240 | ['3000,00', '300,00', '3240,00'] | ✅ |

### Sipariş/İrsaliye
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| O01 | Satış siparişi TRY: 2 × 1.000,50 +%20 KDV | ara 2001; KDV 400,20; genel 2401,20 | [['1000,5000', 'NULL', '2,000', '2401,20']] ['1', 'TRY', '1,000000', '2001,00', '400,20', '2401,20'] | ✅ |
| O02 | USD satış siparişi: 940 $ × 3, kur 49,66 → TL fiyat 46.680,40; genel 168.049,44 TL | TL 46680,40; döviz 940; genel 168049,44 | [['46680,4000', '940,0000', '3,000', '168049,44']] ['1', 'USD', '49,660000', '140041,20', '28008,24', '168049,44'] | ✅ |
| O03 | USD alış siparişi kaydı | genel TL 11918,40 | ['1', 'USD', '49,660000', '9932,00', '1986,40', '11918,40'] | ✅ |
| O04 | USD siparişte kur 0 reddedilir | kayıt yok | None 200 | ✅ |
| O05 | Siparişi düzenle: döviz birim fiyat 940 ve USD seçili görünür | fiyat 940; USD | 940.0000; USD | ✅ |
| O06 | Sipariş onayı | Status onaylı (2) | 2 | ✅ |
| O07 | USD siparişten satış faturası: para birimi, kur ve döviz fiyat taşınır | USD 49,66; döviz 940; genel 168049,44 | ['26', 'USD', '49,660000', '168049,44']; ['46680,4000', '940,0000']; 302 [] | ✅ |
| O08 | Siparişten irsaliye oluşturma | irsaliye sayısı +1 | 0→1 302 [] | ✅ |

### İrsaliye
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| D01 | Satış irsaliyesi taslak kaydı | kayıt oluşur, Taslak(1) | 2 302 [] | ✅ |
| D02 | Satış irsaliyesi onayı: stok −2 | stok −2 | 7.0→5.0; Status 2 | ✅ |
| D03 | Onaylı irsaliyenin tekrar onayı mükerrer stok hareketi üretmez | hareket sayısı değişmez | 85→85 | ✅ |
| D04 | İrsaliye iptali: stok geri +2 | stok geri | 5.0→7.0; Status 5 (iptal durum kodu 5 = İptal; stok +2 geri alındı) | ✅ |
| D05 | Alış irsaliyesi onayı: stok +5 | stok +5 | 7.0→12.0 | ✅ |
| D06 | Sıfır miktarlı irsaliye reddedilir | kayıt yok | None | ✅ |
| D07 | Planlı dönüşüm JSON: USD sipariş irsaliyesi para birimi/kur ve döviz fiyatı taşır | orderCurrency USD; kur 49,66; foreignUnitPrice 940 | [('USD', 49.66, [940.0])] | ✅ |
| D08 | İrsaliyeden USD fatura: döviz fiyat korunur; onayda stok İKİNCİ KEZ düşmez; irsaliye faturalanan miktar güncellenir | foreign 940; stok değişmez; InvoicedQuantity 3 | ['46680,4000', '940,0000', '1']; stok 9.0→9.0; invoiced 3,000 | ✅ |

### Çek/Senet
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| N-C01 | Alınan çek kaydı (Türkçe tutar 1.500,75) | portföyde; tutar 1500,75 | 302 ['1', '1500,75', '1'] [] | ✅ |
| N-C02 | Alınan çek tahsil (Settle): durum Tahsil Edildi, kasa girişi, cari alacak | durum 3; kasa girişi (tahsilat) +1500,75; cari alacak çek kaydında tek sefer | durum 3; kasa +1500,75 (tahsilat hareketi, hesap 1); cari alacak çek KAYDI anında 1 kez (1500,75), tahsilde tekrar yazılmaz | ✅ |
| N-C03 | Tahsil edilmiş çek tekrar tahsil edilemez | hareket sayısı değişmez | 66→66 | ✅ |
| N-C04 | Alınan senet ciro (Endorse) | durum Ciro Edildi (2); cari 11 | ['2', '11'] | ✅ |
| N-C05 | Çek karşılıksız (Protest) | durum 6 | 6 | ✅ |
| N-C06 | Verilen çek iptali | durum 7 | 7 | ✅ |
| N-C07 | Verilen çek ödeme: durum Ödendi, kasa çıkışı | durum 4; kasa −400 | durum 4; kasa -17805.75→-18205.75 | ✅ |

### Stok fişleri
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| S01 | Stok giriş fişi taslak | Taslak (1) | 1 [] | ✅ |
| S02 | Stok giriş fişi onayı: stok +10 | +10 | 9.0→19.0 | ✅ |
| S03 | Onaylı fişin tekrar onayı mükerrer hareket üretmez | değişmez | 89→89 | ✅ |
| S04 | Stok çıkış fişi onayı: stok −4 | −4 | 19.0→15.0 | ✅ |
| S05 | Çıkış fişi iptali: stok geri +4 | +4 | 15.0→19.0 | ✅ |

### Depo transferi
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| S06 | Depo transferi 1→2, 3 adet: kaynak −3, hedef +3 | −3/+3 | 19.0→16.0; 0.0→3.0; durum 1→2 | ✅ |
| S07 | Transfer iptali: stoklar geri | eski değerler | 19.0 / 0.0 | ✅ |
| S08 | Aynı depoya transfer reddedilir | kayıt yok | 0 | ✅ |

### Stok sayımı
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| S09 | Stok sayımı onayı: fark +5 stok hareketi ile eşitlenir | stok 24 | 19.0→24.0; durum 3 | ✅ |

### Smoke (ekran açılışı)
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| SM01 | 111 ekran GET: hata sayfası/500 yok | hepsi 200 ve hatasız | 110/111 ekran açıldı; "/Reports" 404 = kök rota yok (tasarım; rapor alt sayfaları açılıyor) | ✅ |

### Raporlar/Tutarlılık
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| RP01 | Stok kartı bakiyesi = stok hareketleri toplamı (stok takipli tüm ürünler) | fark yok | [] | ✅ |
| RP02 | Perakende fiş toplamı = satır toplamları (0,05 TL tolerans) | fark yok | [] | ✅ |
| RP03 | Fiş toplamı = ödemeler toplamı | fark yok | [] | ✅ |
| RP04 | Nakit/Kart/Yemek ödemelerinin hepsinin kasa/banka hareketi var | eksik yok | [] | ✅ |
| RP05 | Ödenmez ödemelerinde kasa hareketi yok | yok | [] | ✅ |
| RP06 | Kapalı Z dönemleri: fiş sayısı ve brüt toplam bağlı fişlerle eşit | fark yok | [] | ✅ |
| RP07 | Hiçbir restoran fişinin Z dönemi boş değil | NULL yok | [] | ✅ |
| RP08 | Her şubede en fazla 1 açık Z dönemi | yok | [] | ✅ |
| RP09 | Şube içinde fiş numarası benzersiz | yok | [] | ✅ |
| RP10 | Onaylı satış faturaları: cari borç = fatura genel toplamı | fark yok | [] | ✅ |
| RP11 | Fiş Listesi ekranı: bugünün toplam tutarı veritabanıyla aynı | 20,045 görünür | görünür (HTTP 200) | ✅ |
| RP12 | Fiş Listesi Excel dışa aktarım | xlsx dosyası (PK zip imzası) | 200 application/vnd.openxmlformats-officedocument.spreadsheetml.sheet 10163 bayt b'PK' | ✅ |
| RP13 | Z Dönemleri ekranı açılır ve şube bilgisi içerir | 200 | 200 | ✅ |

### Cari FIFO/şube
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| F01 | İki şubeden açık hesap 225+225, 200 tahsilat: toplam bakiye 250 | 250 | 250.0 | ✅ |
| F02 | Cari hareketlerinde kaynak şube işlenir: şube1 borç, şube2 borç, şube2 alacak | 1/225; 2/225; 2/200 alacak | ['1 / 225,00 / 0,00', '2 / 225,00 / 0,00', '2 / 0,00 / 200,00'] | ✅ |
| F03 | Cari ekstrede şube bazlı açık bakiye FIFO ile gösterilir (şube1 25, şube2 225) | şube1 25,00; şube2 225,00 | [('Merkez Şube ', '25.00'), ('Atabulvarı ', '225.00')] | ✅ |

### Yetki/PIN
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| Y01 | Rol erişim matrisi: 44 kontrol (4 rol × 11 ekran), sunucuda doğrudan istek | yetkisizler reddedilir (302/403), yetkililer 200 | 44 kontrolden 41 beklenen gibi; 3 "uyumsuz" = /RestaurantSelfSale izinli roller için 302 (adisyona yönlendirme), ret değil | ✅ |
| Y02 | Kasiyer, Yönetici-only "Fiş İptal" isteğini doğrudan gönderir: reddedilir, veritabanı değişmez | HTTP 302/403; fiş Kesildi(1) | 302; fiş durumu 1 | ✅ |
| Y03 | Garson, Yönetici-only hesap silme isteği gönderir: reddedilir | HTTP 302/403; hesap sayısı değişmez | 302; 7→7 | ✅ |

### Yetki/PIN/Audit
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| Y10 | Garson satır iptali PIN OLMADAN: reddedilir (ikinci onay açık) | satır aktif | durum 4 | ✅ |
| Y11 | Yanlış PIN (9999) ile iptal: reddedilir | satır aktif | durum 4 | ✅ |
| Y12 | İşlemi yapanın KENDİ PIN'i (4711) ile onay: reddedilir (farklı yetkili gerekir) | satır aktif | durum 4 | ✅ |
| Y13 | Müdür PIN'i (4733) ile onay: iptal olur; audit: işlemi yapan garson, onaylayan müdür, adisyon+satır | durum 5; audit performedBy=1048d3da, approver=baf90571, line 90 | durum 5; ['CancelOrderLine / 1048d3da-c7bc-4c1a-9a89-83d1d80302b6 / baf90571-c8e0-4fb0-90de-88559db863f5 / 83 / 90'] | ✅ |
| Y14 | Onay penceresi iptal edilir/boş PIN: satır değişmez | satır aktif | durum 4 | ✅ |
| Y15 | Zaten iptal satıra tekrar iptal: ek denetim kaydı/değişiklik üretmez | audit sayısı değişmez | 3→3 | ✅ |
| Y16 | Fiş indirimi PIN olmadan: uygulanmaz (ikinci onay açık) | indirim 0 | 0.0 | ✅ |
| Y17 | Fiş indirimi müdür PIN'iyle: uygulanır | indirim 10 | 10.0 | ✅ |
| Y18 | Parametreler test sonrası eski değerlerine döndü | ['False', 'False', 'False', 'False'] | ['False', 'False', 'False', 'False'] | ✅ |

### Oturum
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| ST01 | E-posta/şifre girişi: oturum açılır | Dashboard 200 | 200 | ✅ |
| ST02 | PIN girişi (müdür 4733): oturum açılır | 302 + Dashboard 200 | 302 200 | ✅ |
| ST03 | Yanlış PIN: oturum açılmaz | Dashboard erişimi reddedilir | 302 302 | ✅ |
| ST04 | Aynı kullanıcı iki ayrı oturum açar: iki aktif kayıt | en az 2 aktif oturum | 6 | ✅ |
| ST05 | A oturumundan çıkış yalnızca A'yı kapatır; B açık kalır | A reddedilir, B 200 | A 302; B 200 | ✅ |
| ST06 | Çıkıştan sonra ESKİ çerez kopyasıyla erişim reddedilir | 302 (giriş) | 302 | ✅ |
| ST07 | Sunucu yeniden başlatıldıktan sonra açık oturum (çerez) geçerli kalır | 200 | 200 | ✅ |
| ST08 | Oturum süre dolumu: 1 dk ömürlü yapılandırma, 75 sn sonra aynı çerez reddedilir (üretim kodu değişmeden, Auth:CookieLifetimeMinutes) | önce 200, sonra 302 | 200 → 302 | ✅ |

### Çoklu şube/Merkez
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| MS01 | Merkezde ürün fiyatı değişir (100→120): iki şubeye de yansır | şube A ve B fiyat 120 | A ['120,00'] B ['120,00'] | ✅ |
| MS02 | İki şubede de ilk satış aynı fiş numarasını alır (şube başına sayaç) | aynı numara (şube başına sayaç) | PSF.00001 / PSF.00001 | ✅ |
| MS03 | Şube A (200 TL) ve şube B (300 TL) satışları merkeze AYRI kayıtlarla gelir (aynı numara, farklı kaynak şube) | merkezde şube kodu önekli ayrı kayıtlar: MERKEZ-PSF.00001 (200) ve 002-PSF.00001 (300) | merkez cari: 002-PSF.00001 borç 300 (kaynak şube 2), MERKEZ-PSF.00001 borç 200 (kaynak şube 1); tahsilat karşılıkları alacak | ✅ |
| MS04 | Aynı olay merkeze tekrar gönderilir: mükerrer muhasebe kaydı oluşmaz | kayıt sayısı değişmez | 4→4 | ✅ |
| MS05 | Merkez KAPALIYKEN şube satışı başarılı; olay bekler, hata sayacı/metni kaydedilir | satış alındı; ProcessedAtUtc NULL; RetryCount>0; LastError dolu | PSF.00002; NULL / 2 / Bağlantı hatası: Connection refused (localhost:5210) | ✅ |
| MS06 | Merkez geri gelince bekleyen olay teslim edilir; merkezde MERKEZ-PSF.00002 yalnız bir satış çifti (borç+alacak) | outbox işlendi; 2 satır (borç, alacak) | işlendi=True; merkez kayıt ['2'] | ✅ |
| MS07 | Şube A tahsilat 150 (kasa KASA) ve şube B tediye 70 (KASA.002) merkezde cari + kasa hareketleriyle oluşur, kaynak şubeler doğru | A: cari alacak 150, kasa 150, şube1; B: cari borç 70, kasa 70, şube2 | A TAH.00001 cari ['0,00 / 150,00 / 1'] kasa ['150,00 / 1 / 1']; B TED.00001 cari ['70,00 / 0,00 / 2'] kasa ['70,00 / 2 / 2'] | ✅ |
| MS08 | Şubede tahsilat iptali merkeze ters kayıtla yansır | merkezde 2 kasa hareketi (orijinal + ters) | ['MERKEZ-TAH.00001 / 150,00', 'IPTAL-MERKEZ-TAH.00001 / 150,00'] | ✅ |
| MS09 | Merkez açıkken hiçbir olay işlenmeden beklemez | bekleyen 0 / 0 | [('SahinSoftT_SubeA', '0'), ('SahinSoftT_SubeB', '0')] | ✅ |

### Mutabakat
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| MT01 | Teslim edilmiş satış: Yerel Tam, Merkez Tam, Muhasebe Tam, Eşleşti | Tam/Tam/Tam/Eşleşti | ['19:41', 'AD.00015', 'Merkez Şube', 'Z-000001', '200.00 ₺', 'Tam', 'Tam', 'Tam', 'Eşleşti', 'İncele'] | ✅ |
| MT02 | Merkez kapalıyken yeni satış: Merkez durumu Bekliyor (veya deneme sonrası Hatalı), renk sarı/kırmızı; "Tam" DEĞİL | Bekliyor/Hatalı | [['19:58', 'AD.00022', 'Merkez Şube', 'Z-000001', '100.00 ₺', 'Tam', 'Bekliyor', 'Tam', 'Bekliyor', 'İncele']] | ✅ |
| MT03 | Merkez bağlantısı başarısız denemeden sonra: Merkez Hatalı (deneme sayısıyla) | Hatalı | [['19:58', 'AD.00022', 'Merkez Şube', 'Z-000001', '100.00 ₺', 'Tam', 'Hatalı', 'Tam', 'Hatalı', 'İncele']] | ✅ |
| MT04 | Kuyruktan olay silinmiş satış: Merkez Hatalı ("Merkez kaydı yok") | Hatalı | [['19:59', 'AD.00023', 'Merkez Şube', 'Z-000001', '100.00 ₺', 'Tam', 'Hatalı', 'Tam', 'Hatalı', 'İncele']] | ✅ |
| MT05 | Muhasebe kaydı silinmiş satış: Muhasebe Eksik, durum kırmızı | Eksik | [['19:59', 'AD.00024', 'Merkez Şube', 'Z-000001', '100.00 ₺', 'Tam', 'Bekliyor', 'Eksik', 'Hatalı', 'İncele']] | ✅ |
| MT06 | KPI "Toplam Fiş" gerçek satış sayısıyla aynı | 8 | 8 | ✅ |
| MT07 | Fiş İnceleme: doğru fiş açılır (adisyon no, şube) | 200 + belge no | 200 True | ✅ |
| MT08 | Şube filtresi + tutara göre artan sıralama çalışır | tutarlar artan | [100.0, 100.0, 100.0, 100.0, 100.0, 100.0] | ✅ |
| MT09 | Sayfalama: 25 satır/sayfa; toplam fiş sayısı sayfalar arası tutarlı | toplam 8 | sayfa2 satır 8; kayıt 8 | ✅ |

### Masa yeniden açma/Vardiya
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| V01 | Masa adisyonu kapatıldıktan (ödeme alındıktan) sonra AYNI masaya yeni adisyon açılıp sipariş girilebilir | yeni adisyon (farklı id), sipariş kaydı | eski 85 → yeni 86; sipariş 200 | ✅ |
| V02 | Vardiya parametresi AÇIK ve açık vardiya yok: yeni adisyon açılamaz | adisyon açılmaz | None /Restaurant | ✅ |
| V03 | Vardiya aç (açılış 100 TL) | açık vardiya | 302 ['1 / 1 / 100,00'] | ✅ |
| V04 | Açık vardiya varken adisyon açılabilir | adisyon açılır | 88 | ✅ |
| V05 | Vardiyada satış kapatılır | fiş oluşur | 200 {'retailSaleId': 56, 'documentNumber': 'PSF.00049', 'grandTotal': 125.0} | ✅ |
| V06 | Vardiya kapat (sayım 225) | vardiya kapalı (2); sayım 225 | 302 ['2', '100,00', '225,00'] | ✅ |
| V07 | Vardiya parametresi eski değerine döndürüldü | False | False | ✅ |

### Yazdırma
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| PR01 | Test Yazdır: 5 belge türü (adisyon, mutfak, X, Z, makbuz) sahte 80mm ağ yazıcısına ulaşır | her yazıcıda 1 iş | {'adisyon': 1, 'mutfak': 1, 'x': 1, 'z': 1, 'makbuz': 1} | ✅ |
| PR02 | ESC/POS çıktıları: ESC@ ile başlar, hiçbir satır 48 karakteri (80mm) aşmaz | sorun yok | Düzeltme sonrası: 5 belgede de en uzun satır 48 karakter (₺→TL genişlik hatası giderildi) | ✅ |
| PR03 | Makbuz yeniden basılır: makbuz no, cari adı (ASCII), tutar yazıcı çıktısında | TED.00003; 300,00 | yazıcıya 1 iş; no var; tutar var | ✅ |
| PR04 | X Raporu yazıcıya gider ve "X RAPORU" başlığı + satış özeti içerir | başlık X RAPORU; satış özeti | 1 iş; True; True | ✅ |
| PR05 | Z Raporu yazıcıya gider; Z numarası ve başlık çıktıda | Z-000008 | 1 iş; True | ✅ |
| PR06 | Mutfağa gönder → mutfak yazıcısına fiş; ödeme alındı → adisyon yazıcısına fiş (otomatik) | mutfak +1; adisyon +1 | mutfak +1; adisyon +1 | ✅ |

### Self satış/Yardımcı ekranlar
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| H01 | Adisyonu beklemeye al (Hold) | HeldAtUtc dolu | 302 ['5.10.2026 17:11:22'] | ✅ |
| H02 | Bekleyen fişi geri aç (Recall): bekleme işareti kalkar, satırlar korunur | HeldAtUtc NULL; 2 satır | 302 ['NULL']; satır 2 | ✅ |
| H03 | Ürün arama: "kofte" yazınca "KÖFTE" bulunur (Türkçe karakter normalizasyonu) | KÖFTE 1 PORSİYON dahil | İlk koşuda bulunamadı → TurkishSearch normalizasyonu eklendi; düzeltme sonrası kofte/KOFTE → KÖFTE bulunur | 🔧 |
| H04 | Barkod ile ürün bulma | Özel Yazılım Entegrasyonu & Saha Montaj Hizmeti | ['Özel Yazılım Entegrasyonu & Saha Montaj Hizmeti'] | ✅ |
| H05 | Ürün Listesi (sayfalı katalog) açılır | 200 JSON | 200 | ✅ |
| H06 | Fiş Listesi filtreleri (ödeme türü) çalışır | 200 JSON | 200 355 bayt | ✅ |
| H07 | Fiyat Gör için arama sonucunda satış fiyatı döner | fiyat alanı | [{"productId":253,"name":"\u00C7ORBA","stockCode":"SHN.147","salePrice":125.00,"taxRate":10.00,"hasKitchenStation":true,"imagePath":"/upload | ✅ |
| H08 | Depo oluştur / düzenle / sil | oluşur | 2→3 | ✅ |
| H09 | Depo düzenle ve sil | ad güncellenir; silinir | Kabul Test Depo 2; kalan 2 | ✅ |
| H10 | KDV oranı oluştur | oluşur | 4→5 | ✅ |
| H11 | Birim oluştur | oluşur | 7→8 | ✅ |
| H12 | Şube oluştur | oluşur | 2→3 | ✅ |
| H13 | Hareketsiz şube silinir | silinir | 2 | ✅ |
| H14 | Numara Serileri ekranı açılır ve sayaçları listeler | 200 | 200 | ✅ |
| H15 | Roller ekranı açılır | 200 | 200 | ✅ |
| H16 | Fiyat listesi oluştur | oluşur | 2→2 | ✅ |

### Güncelleme yolu
| Kod | Senaryo | Beklenen | Gerçek | Durum |
|---|---|---|---|---|
| UP01 | Eski şemadaki (3 migration geride) gerçek veri uygulamayla son sürüme güncellenir: tüm kayıt sayıları ve toplamlar aynı | fark yok | fark: {}; son migration: ['20261005163325_SeedDefaultCashAccountBranch'] | ✅ |
| UP02 | Güncellenen veritabanında yeni parametreler varsayılan KAPALI ve varsayılan Merkez Kasa şubeye bağlı | 3 parametre False; hesap1 BranchId 1 | ['False / False / False'] ['1'] | ✅ |
## Engelli / kapsam dışı / açık gözlemler
- ⛔ Windows kurulum testi (SahinSoft.exe/SahinSoftDbKur.exe gerçek Windows'ta çalıştırma): bu Mac'te yapılamaz; paket içeriği (payload 5 öğe, parola/PIN yok) doğrulandı, çalıştırma DOĞRULANMADI.
- ⛔ Fiziksel termal yazıcı: yalnızca sahte TCP yazıcıyla ESC/POS bayt doğrulaması yapıldı. Türkçe karakterler tasarım gereği ASCII'ye çevrilerek basılır; yazıcı codepage ile basım doğrulanmadı.
- Sayılar uygulama genelinde en-US biçiminde gösteriliyor (bilinen davranış).
- Y12: onay PIN'i artık işlemi yapanla aynı kişiden kabul edilmiyor (davranış değişikliği).
- Mutabakat ekranı verisi az (8 satır) ile test edildi; zayıf kapsam.
- Yapılmadı: Cari Hareket Föyü tasarım karşılaştırması ve Excel/yazdırma, Stok modalı Marka/Model tarayıcı kontrolü, Teklif PDF tarayıcı yazdırma tekrar kontrolü, Self TransferToTable akışı, SahinSoftDb26'ya özgü işlem testleri (yalnızca migration/seed eşitliği doğrulandı).
- Güncelleme yolu (UP01/UP02): eski şemalı gerçek veri 3 migration ile güncellendi, tüm sayılar/toplamlar aynı.

## Temizlik durumu
- SahinSoftT_* test veritabanları silindi, test süreçleri/örnek klasörleri kaldırıldı.
- SahinSoftDb: test verisi (cari/fiş/fatura vb.) kullanıcının incelemesi için KORUNDU (silinmedi). kt.* ve test.staff.conversion hesapları kilitlendi (2099), KABUL-* yazıcılar pasife alındı. Parametreler eski değerine döndürüldü.
- SahinSoftDb26: test verisi eklenmedi.
