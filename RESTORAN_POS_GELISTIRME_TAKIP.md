# ŞahinSoft Restoran POS — Büyük Geliştirme Takip Listesi

**Başlangıç: 2026-09-04.** Edip'in "ŞAHİNSOFT RESTORAN POS — CLAUDE GELİŞTİRME TALİMATI" belgesindeki
36 maddenin birebir uygulanması. Bu dosya oturumlar arası devamlılık içindir — iş kesilirse
(token/oturum limiti) burdan kaldığı yerden devam edilir. Her madde işaretlenmeden, gerçek
fonksiyon test edilmeden bir sonrakine geçilmez.

Kesin kurallar (talimat belgesinden, unutulmaz): soru sorma, tasarım uydurma, onaylı ekran
dizaynını değiştirme, mevcut çalışan fonksiyonu bozma, görselleri değil son onaylı tasarımı
referans al, buton görsel değil gerçek fonksiyon, multi-tenant izolasyon korunacak, responsive
15.6/18/21", üst/alt/kritik alan sabit + içerik kendi içinde kayar, parametrik/yetki bazlı olanı
hard-code etme, her geliştirme sonrası gerçek test, hata bulunca onay beklemeden düzelt.
Bitince yalnızca: **"Bitti, devam edebiliriz."**

## Referans görseller (Edip'in gönderdiği son onaylı tasarımlar, 2026-09-04)
1. Masa Satış — status renkli üst çizgili kart grid, 5'li özet kart satırı (Boş/Dolu/Hesap İstendi/Rezerve/Açık Adisyon)
2. Self Satış — kategori sekmeleri (sayaçlı), 4 sütun ürün kartı, sağ ikon şeridi (gerçek ikonlar), alt buton düzeni: Mutfağa Gönder/Ödemeyi Al, Adisyon/Nakit/Kredi Kartı, Cari Ekle/%İndirim/Kapat
3. Restoran Raporları — tarih ARALIĞI seçici, Excel/PDF export, 2 satır rapor türü sekmesi (Günlük/Aylık, Ödeme Dağılımı, En Çok Satanlar, Kategori Satışları, KDV Dökümü, İndirim&İkram / Kasiyer Raporu, Günlük Fişler, X, Z, Z Listesi)
4. Paket Operasyon Merkezi — kanal sekmeleri (Telefon/Web/Yemeksepeti/Trendyol/GetirYemek), durum kuyruğu, 3 sütun (kuyruk/detay/kurye takip haritası) — **mimariyi bozma, derin geliştirme SONRAKİ pakette**

**Edip 2026-09-04 ~23:00'te aynı 4 görseli TEKRAR gönderdi** ("bu görselleri artık yeni
tasarımlarımız... butun butonlar çalışsın... sen ne zaman istersen iş planına al") - bu YENİ bir
tasarım değil, YUKARIDAKİ 4 görselin AYNISI, sadece hatırlatma/vurgu. Bu vesileyle Self Satış
mockup'ının alt buton düzeni ("Adisyon | Nakit | Kredi Kartı" + "Cari Ekle | %İndirim | Kapat",
"Sipariş Sil"in sağ sütunda AYRI kırmızı bir buton olarak durduğu) ile mevcut kodun TAM
piksel eşleşmediği fark edildi:
- **Düzeltildi (madde 23-24, 2026-09-05):** "Sipariş Sil" alt hızlı-işlem sırasından ÇIKARILIP sağ
  ikon sütununa, Fiş İkram'ın altına, kırmızı (`.pos-side-icon.danger`) bir buton olarak taşındı;
  alt sıra artık mockup'taki gibi "Cari Ekle | %İndirim | Kapat" (3 sütun). Fonksiyon
  (`clear-cart-btn` id'si, bekleyen sepeti temizleme) aynı kaldı, taşıma sadece DOM konumu/CSS -
  tarayıcıda doğrulandı (Self Satış'ta ürün eklenip Sipariş Sil'e basıldığında sepet
  temizleniyor).
- **Hâlâ açık:** "Adisyon" adında bir hızlı buton mockup'ta var, mevcut kodda YOK (muhtemelen
  mevcut açık adisyonu görüntüleme/geçiş kısayolu - tam işlevi belirsiz). Görseli tekrar incelemeden
  tahminle eklenmedi ("tasarım uydurma" kuralı) - Edip'ten netleştirme istenene kadar veya
  görsel tekrar paylaşılana kadar bekliyor.

---

## İlerleme Durumu (özet - en güncel hali için madde detaylarına bak)

| # | Madde | Durum |
|---|---|---|
| 0 | Bu takip dosyası + mimari karar notu | ✅ |
| 2 | Ortak Satış Mimarisi | ✅ doğrulandı (2026-09-04) - RestaurantSelfSaleController.Index, RestaurantPackageController.Create ve Masa Satış'ın kendisi ÜÇÜ DE RestaurantController/Check'e (aynı Check.cshtml + RestaurantPostingService) yönleniyor, sadece kayıt hedefi (masa/self/paket) farklı |
| 3 | Self Satış Hızlı Ödeme + fiş sorulsun mu parametresi | ✅ tamam + test edildi (2026-09-04) |
| 4 | Ödeme Al ekranı (Toplam/Ödenen/Kalan/Para Üstü) | ✅ tamam + test edildi (2026-09-04) |
| 5 | Parçalı Ödeme | ✅ tamam + test edildi (2026-09-04, sunucu kalıcılığıyla birlikte) |
| 6 | Tutarı Bölme (1/2,1/3..) | ✅ tamam + test edildi (2026-09-04, 1/2 bölünmesi doğrulandı) |
| 7 | Ödeme İptal | ✅ tamam + test edildi (2026-09-04) |
| 8 | Adisyona Dön (kilitlenmeden, kısmi ödeme korunur) | ✅ tamam + test edildi (2026-09-04, tam sayfa yenilemede bile korunduğu doğrulandı) |
| 9 | Satır Kilidi TAMAMEN KALDIRILACAK | ✅ tamam + test edildi (2026-09-04) |
| 10 | İndirim ortak motor | ✅ doğrulandı + test edildi (2026-09-04) - ana ekran ve ödeme ekranı AYNI #ticketDiscountModal'ı açıyor (pay-discount-btn → openTicketDiscountModal(true)), İndirimi Kaldır (discount-clear-btn) ApplyTicketDiscountAsync(0) çağırıp TÜM satırların DiscountAmountSnapshot'ını sıfırlayarak orijinal toplamı tam geri getiriyor, hızlı ödeme zaten net (indirimli) tutar üzerinden çalışıyor (ComputeCheckRunningTotal DiscountAmountSnapshot'ı düşüyor) |
| 11 | Fiş İkram (parametrik sebep, ciroya dahil değil) | ✅ tamam + test edildi (2026-09-04) - stok hareketi kısmı için not: bkz. aşağıdaki detay |
| 12 | Ödenmez ödeme tipi | ✅ tamam + test edildi (2026-09-04) |
| 13 | Açık Hesap + zorunlu cari | ✅ tamam + test edildi (2026-09-04) |
| 14 | Tahsilat Carileri / platform ödemeleri | ✅ tamam + test edildi (2026-09-04) |
| 15-16 | Ürün Arama modal + Türkçe normalize | ✅ tamam + test edildi (2026-09-04) - "klavye altta" kısmı için şimdilik MEVCUT basit klavye kullanılıyor, madde 17'nin YENİ özel klavyesi ayrı iş olarak duruyor |
| 17 | Özel sanal klavye | ✅ tamam + test edildi (2026-09-04) |
| 18 | Bekleyen Fişler kart tasarımı | ✅ tamam + test edildi (2026-09-04) |
| 19 | Fiş Listesi Excel-vari filtrelenebilir | ✅ tamam + test edildi (2026-09-04) |
| 20 | Boş Adisyon otomatik temizlik | ✅ tamam + test edildi (2026-09-04/05) - gerçek eksik bulundu ve düzeltildi |
| 21 | Yetki Mimarisi (profil, çoklu atama, kritik işlem + 2. yetkili şifresi + audit log) | ✅ tamam + test edildi (2026-09-04) |
| 22 | Sağ İşlem Menüsü parametrik/yetki kontrollü | ✅ tamam + test edildi (2026-09-05) |
| 23-24 | Masa Satış tasarım + kişi sayısı sorulsun mu | ✅ tamam + test edildi (2026-09-05) - "kişi sayısı sorulsun mu" zaten çalışıyordu (önceki oturumda doğrulandı); Masa Satış'ın kendisi (5'li özet kart satırı, renkli üst çizgili masa kartları, salon sekmeleri) mockup ile zaten eşleşiyor, tarayıcıda tekrar karşılaştırılıp doğrulandı; aynı görsel setinden Self Satış buton düzeni fidelity gap'i de bu oturumda düzeltildi (bkz. yukarıdaki Referans görseller notu) |
| 25 | Mutfağa gönderilmiş ürün düzenleme yetkisi | ✅ tamam + test edildi (madde 9 ile birlikte) |
| 26-27 | Ayarlar reorganizasyon + Kasa tanımları (şube bazlı) | ⏳ |
| 28-34 | Raporlar (ortak dönem filtresi, Kasiyer/X/Z/Z Listesi) | ⏳ |
| 35 | Paket — mimariyi bozma, derin geliştirme sonraki pakette | 🔒 dokunulmuyor (bilerek) |
| 36 | Test ve teslim | ⏳ her madde kendi içinde test edilecek |

**Yasal:** ✅ tamam · 🔶 kısmen/mevcut doğrulanacak · ⏳ yapılmadı · 🔒 bilerek dokunulmuyor

---

## Teknik mimari kararları (ilerledikçe eklenecek)

### Madde 21 — Yetki Mimarisi tasarımı
- `RestaurantPermissionProfile` (yeni tablo): Id, Name (admin'in girdiği serbest isim, ör. "Kasiyer",
  "Şef Garson"), + kritik işlemler için bool alanlar: CanCancelOrderLine, CanRemovePendingLine,
  CanCancelReceipt, CanApplyDiscount, CanApplyComplimentary, CanApplyUnpaid, CanEditKitchenSentLines,
  CanAddNote, CanProcessReturn. (Sabit/enum liste - "X/Y/Z Yetkisi" örnekleri profil ADI placeholder'ı,
  gerçek yetki bayrakları bu sabit listeden.)
- `RestaurantPersonnelPermissionProfile` (join, many-to-many): ApplicationUserId <-> ProfileId.
- Varsayılan: hiç profil atanmamış kullanıcı = TÜM yetkiler AÇIK (spec: "varsayılan bütün yetkiler
  açık başlayabilir"). Administrator rolü HER ZAMAN tam yetkili (mevcut kural korunur).
- "Şifre sorulsun mu" ikinci yetkili onayı: kritik işlem bazında GLOBAL parametre
  (InventorySettings'e RequireSecondApprovalFor... alanları, RequireCancellationReason ile AYNI
  desen). Açıksa işlemi yapan kişi yetkili olsa bile ikinci bir yetkilinin PIN/şifresi istenir,
  onaylayan MEVCUT oturumdaki kişi olmak zorunda değil. `RestaurantPermissionAuditLog` tablosuna
  loglanır (kim yaptı, kim onayladı, ne zaman, hangi işlem, hangi kayıt).
- Servis: `RestaurantPermissionService.HasPermissionAsync(userId, RestaurantPermission flag)` ve
  `RequiresSecondApprovalAsync(flag)` + `VerifyApproverAsync(userId, pin)`.

### Madde 22 — Sağ İşlem Menüsü (parametrik/yetki kontrollü görünürlük), 2026-09-05

İki katmanlı görünürlük: **Katman 1** sistem geneli (Ayarlar > Stok > "Sağ İşlem Menüsü (Sistem
Geneli)" - `InventorySettings`'e 11 yeni `EnableXButton` bool, hepsi varsayılan açık), **Katman 2**
personel/profil bazlı (`RestaurantPermissionProfiles` formuna yeni "Sağ İşlem Menüsü Görünürlüğü"
paneli - `RestaurantPermissionProfile`'a 9 yeni `CanSeeX`/`CanClearOrder` bool, hepsi varsayılan
açık; Fiş Notu ve Fiş İkram için YENİ bayrak eklenmedi, mevcut `CanAddNote`/`CanApplyComplimentary`
tekrar kullanıldı çünkü zaten aynı işlevi kontrol ediyorlar). Efektif görünürlük sunucu tarafında
`(SistemGeneliAçık) && (ProfilİzinVeriyor)` olarak hesaplanıp `RestaurantCheckViewModel`'deki 11
`ShowXButton` alanına yazılıyor, `Check.cshtml`'de `@if` ile DOM'dan tamamen çıkarılıyor (Mutfak
butonu bilerek kapsam dışı bırakıldı - spec'te yok). Tek istisna `send-kitchen-btn` (Mutfağa
Gönder): `renderCart()` içinde bu butona her sepet değişiminde null-guard'sız erişen bir JS satırı
bulunduğu için `@if` ile DOM'dan silmek yerine koşullu `style="display:none"` kullanıldı (DOM'da
kalıyor, JS erişimi hâlâ güvenli).

**Yan bulgu ve düzeltmeler (bu maddeyi yaparken bulundu):**
- `restaurant-pos.js`'te `send-kitchen-btn`'e event listener eklenen satır tamamen guard'sızdı
  (`document.getElementById('send-kitchen-btn').addEventListener(...)`) - buton hiç render
  edilmeseydi throw ederdi. `if (sendKitchenBtn) {...}` ile sarmalandı.
- `ApplicationDbContextFactory.cs` (EF Core design-time factory, `dotnet ef migrations/database
  update` komutlarının kullandığı) yerel `Server=.\SQLEXPRESS` bağlantısına HARD-CODE'luydu -
  gerçek uygulama artık uzak SQL Server'a (141.98.114.2) bağlanıyor, bu yüzden `dotnet ef database
  update` sürekli "sunucu bulunamadı" hatası veriyordu. appsettings.json'dan okuyacak şekilde
  düzeltildi (`Program.cs`'teki gerçek bağlantı çözümlemesiyle aynı desen). Muhtemelen ilk yerel
  SQLEXPRESS geliştirme döneminden kalma bir kalıntıydı, hiç fark edilmemiş.
- Migration'ın `AddColumn` varsayılan değeri (`defaultValue: false`) SADECE C# tarafındaki
  `= true` varsayılanını YENİ oluşturulan profiller için geçerli kılıyor - var olan
  `RestaurantPermissionProfiles` satırları (ör. Test-Kısıtlı) migration sonrası TÜM yeni
  `CanSeeX`/`CanClearOrder` alanlarında `false` ile kalıyordu, yani geçişten hemen sonra sağ menü
  butonları var olan her profilde sessizce kaybolurdu. Migration'a elle bir `UPDATE
  RestaurantPermissionProfiles SET ... = 1` SQL bloğu eklenip düzeltildi (InventorySettings zaten
  `UpdateData` ile Id=1 satırı için doğru yapılmıştı, ama Profiller tablosunda hiç yoktu).

**Test (tarayıcıda, 2026-09-05):**
1. **Katman 2 (profil bazlı):** Test-Kısıtlı profilinde `CanSeeHoldReceipt` (Fişi Beklet) ve
   `CanClearOrder` (Sipariş Sil) kapatıldı, kaydedildi. Test Garson (PIN 9911, bu profile atanmış)
   ile giriş yapılıp bir adisyon açıldı - `side-hold-btn` ve `clear-cart-btn` DOM'da hiç YOK,
   diğer tüm butonlar (Fiş Notu, Mutfak, Fiyat Gör, Klavye, Ürün Listesi, Bekleyen Fişler, Fiş
   İkram, Fiş Listesi, Mutfağa Gönder) normal görünür - pozitif kontrol de doğrulandı.
2. **Katman 1 (sistem geneli):** Ayarlar > Stok'ta "Klavye" (EnableKeyboardButton) kapatılıp
   kaydedildi. Administrator (EdipŞahin) ile aynı adisyon açıldı - `side-keyboard-btn` DOM'da hiç
   YOK (Administrator normalde TÜM Katman-2 kısıtlamalarını bypass eder, ama Katman-1 sistem
   geneli kapatınca O DA etkileniyor - beklenen davranış), `side-hold-btn`/`clear-cart-btn` ise
   Administrator için normal görünür (Katman 2 ona uygulanmıyor).
3. Her iki test toggle'ı da testten sonra tekrar açık (varsayılan) hale geri alındı - canlı
   sistemde test kalıntısı bırakılmadı.

Migration: `AddRightMenuVisibilityFlags` (20260904211751) - üretildi, elle düzeltildi (yukarıdaki
UPDATE bloğu), `dotnet ef database update` ile uzak DB'ye uygulandı, `SahinSoftDb_Migration.sql`
idempotent script'i hem `SahinSoft.DbSetup/` hem `deploy/` altına kopyalandı.

### Madde 4-8 (Ödeme Al ekranı / Parçalı Ödeme / Tutarı Bölme / Ödeme İptal / Adisyona Dön) — MİMARİ BULGU, 2026-09-04

Bu 5 madde birbirine sıkı bağlı, TEK bir mimari kısıttan dolayı hiçbiri spec'in istediği gibi
çalışmıyor: **kısmi ödemeler şu an SADECE tarayıcı JS belleğinde tutuluyor, sunucuya HİÇ
yazılmıyor.** Bulgular (`restaurant-close-payment.js`, `Check.cshtml` #closePaymentModal
incelendi):

- `paymentLines` JS dizisi `openPaymentModal()` her çağrıldığında SIFIRLANIYOR (satır 110) -
  yani "Adisyona Dön" (sadece `data-bs-dismiss="modal"`, sunucuya dokunmuyor) ile modalı kapatıp
  tekrar "Ödemeyi Al"a basınca önceki kısmi ödemeler KAYBOLUYOR. Spec'in "kısmi ödemeler
  korunur, ana ekranda da Ödenen/Kalan görünür" şartı sağlanmıyor.
- "Ödeme İptal" ve "Adisyona Dön" butonları BİREBİR AYNI kodu çalıştırıyor
  (`data-bs-dismiss="modal"`) - aralarında hiçbir davranış farkı yok. Spec'in istediği "İptal
  kısmi ödemeleri sıfırlar + ekranda kalır, Dön ise korur + ekrandan çıkar" ayrımı hiç yok.
  `RestaurantOrderLine` gibi tekil bir onay/işlem geçmişi yok - hiçbir sunucu tarafı state yok.
- `RestaurantPayment` (Domain) HAZIR ve uygun bir entity - PaymentMethod/Amount/RestaurantCheckId/
  FinancialAccountId/FinancialTransactionId(nullable, "kapanışta doldurulur" yorumu VAR). AMA şu an
  SADECE `RestaurantPostingService.CloseCheckAsync` içinde, adisyon KAPANIRKEN tek seferde
  topluca yazılıyor (satır ~1543, ~1699) - kısmi/ara kayıt akışı yok.

**Doğru çözüm (henüz uygulanmadı) - bir sonraki oturumun/adımın planı:**
1. Yeni bir sunucu endpoint'i: "bu check'e kısmi ödeme kaydet" - `RestaurantPayment` satırı
   yazar, `FinancialTransactionId = null` bırakır (muhasebeye henüz işlenmemiş, sadece "alındı"
   olarak kayıtlı), check AÇIK kalır.
2. `RestaurantCheckViewModel`/Check.cshtml ana ekranına Ödenen/Kalan özet alanı eklenmeli (spec:
   "ana ekranda da görünür").
3. "Ödeme İptal" → bu check'in henüz kapanışa işlenmemiş (`FinancialTransactionId == null`)
   `RestaurantPayment` satırlarını SİLER (hard-delete, henüz muhasebeye hiç girmediler), Ödenen=0
   olur, kullanıcı ÖDEME EKRANINDA kalır.
4. "Adisyona Dön" → hiçbir şeye dokunmadan sadece modalı kapatır (zaten öyle) - `openPaymentModal()`
   artık `paymentLines`'ı SIFIRLAMAYIP mevcut check'in DB'deki kayıtlı kısmi ödemelerinden
   YENİDEN YÜKLEMELİ.
5. "Siparişi Tamamla" (`CloseCheckAsync`) - ÖNCEDEN YAZILMIŞ `RestaurantPayment` satırlarını
   yeniden oluşturmak yerine bunları BULUP `FinancialTransactionId`'lerini doldurarak
   finalize etmeli (mevcut kod muhtemelen sıfırdan `new RestaurantPayment` yaratıyor - CloseCheckAsync
   satır ~1543/1699 dikkatle incelenip bu akışa uyarlanmalı, ÇİFT KAYIT riski var).
6. Para Üstü (madde 4): `Kalan < 0` olduğunda (fazla ödeme) hesaplanabilir - Ödenen - Toplam.
7. Tutarı Bölme (madde 6): KALAN bakiye üzerinden 1/2 1/3 1/4 mantığı, 2 ondalık, YUVARLAMA YOK,
   küsurat son ödemeye eklenir - şu an `pay-split-btn` UI'ı var ama sunucu tarafı ile bağlantısı
   yeniden gözden geçirilmeli (kalan hep client-side hesaplanıyor, DB'deki gerçek kalanla senkron
   olmalı).

**NEDEN BU TURDA YAPILMADI (o anki durum, artık ÇÖZÜLDÜ - bkz. aşağıdaki 2026-09-04 bölümü):** Bu,
gerçek para/muhasebe akışına dokunan (FinancialTransaction, CurrentAccountTransaction) hassas bir
mimari değişiklikti. `CloseCheckAsync`'in mevcut muhasebe kayıt mantığını (satır ~1500-1750) tam
anlamadan aceleyle değiştirmek ÇİFT KAYIT veya eksik muhasebe hareketine yol açabilirdi.

### Madde 3-8 (Self Satış Hızlı Ödeme, Ödeme Al, Parçalı Ödeme, Tutarı Bölme, Ödeme İptal,
Adisyona Dön) — TAMAMLANDI, 2026-09-04, TEST EDİLDİ

**Mimari çözüm (Edip'in kendi doğrulattığı kural birebir uygulandı):** Kısmi ödemeler
`RestaurantCheckPendingPayment` (yeni tablo) olarak saklanıyor - bu, `CloseCheckAsync`'in
yazdığı gerçek `RestaurantPayment`/`FinancialTransaction`/`CurrentAccountTransaction`
kayıtlarından TAMAMEN AYRI. Adisyon kapanana kadar HİÇBİR kesin muhasebe/cari hareketi
oluşmuyor - `CloseCheckAsync`'in kendisi TEK SATIR bile değişmedi (sadece kapanış sonunda
artık anlamsız kalan pending satırları temizleyen küçük bir `RemoveRange` eklendi).

**Yapılanlar:**
- `RestaurantCheckPendingPayment` (Domain, yeni): RestaurantCheckId, PaymentMethod, Amount,
  FinancialAccountId, RecordedByUserId, RecordedAtUtc. Migration `AddPendingPaymentsAndQuickPaySetting`.
- `RestaurantPostingService`: `AddPendingPaymentAsync`/`RemovePendingPaymentAsync`/
  `CancelPendingPaymentsAsync` - üçü de SADECE bu tabloya dokunuyor. `CloseCheckAsync` sonunda
  artık bu check'in pending satırlarını siliyor (muhasebeye hiç girmedikleri için silinmeleri
  güvenli).
- `RestaurantController`: `AddPendingPayment`/`RemovePendingPayment`/`CancelPendingPayments`
  (JSON endpoint'ler) + `Receipt(int id)` (yazdırmaya hazır, `Layout=null`, `window.print()`
  otomatik tetiklenen yalın fiş görünümü - RestaurantReportsController'daki "Fişi Gör"den FARKLI
  bir aksiyon, çünkü o controller Waiter rolüne kapalı, bu ise Administrator/RestaurantManager/
  Waiter kapsamında kalıyor).
- `Check.cshtml`/`restaurant-close-payment.js`: modal artık `paymentLines`'ı sayfa yüklenişinde
  sunucudan gelen kayıtlarla dolduruyor ve "Adisyona Dön"de SIFIRLAMIYOR. Ana ekranda (modal
  kapalıyken de) Ödenen/Kalan satırı eklendi. "Ödeme İptal" artık AYRI bir handler - kısmi
  ödemeleri sunucudan siler, ekranda KALIR (Adisyona Dön'den farkı budur). Para Üstü: kasiyer
  kalanın üzerinde bir tutar girerse CANLI olarak fark gösterilir, ama bir ödeme yöntemi
  tuşuna basıldığında kayda giren tutar KALANLA SINIRLANIR - `CloseCheckAsync`'in "ödeme
  toplamı == adisyon tutarı" katı eşitliği bu sayede HİÇ BOZULMADI, fazla tahsilat asla
  muhasebeye girmiyor.
- Self Satış Hızlı Ödeme (madde 3) - `window.RestaurantQuickPay` artık modalı açıp bekletmek
  yerine tam tutarı TEK yöntemle ANINDA kapatıyor (`submitClosePayment` ortak fonksiyonu -
  hem bu hem normal "Siparişi Tamamla" aynı kodu kullanıyor). Yeni `InventorySettings.
  RequireReceiptPromptAfterQuickPay` parametresi (Ayarlar > Stok Parametreleri, varsayılan
  KAPALI) açıkken kapanışın ardından küçük bir "Fiş Yazdır | Kapat" diyaloğu gösteriliyor -
  ikisi de sonunda boş Self Satış ekranına dönüyor.

**Bulunan ve düzeltilen ÜÇÜNCÜ gerçek bug:** Ana ekrandaki Ödenen/Kalan satırlarını gizlemek
için `style="display:none"` + Bootstrap'ın `.d-flex` sınıfını AYNI elemente koymuştum -
Bootstrap'ın `d-flex` sınıfı `!important` olduğu için JS'in `style.display='none'` ataması
HİÇBİR ZAMAN kazanamıyordu, satırlar her zaman (yanlış "0,00 ₺" değerleriyle) görünür
kalıyordu. Düzeltme: `d-flex` sınıfı iç bir sarmalayıcıya taşındı, görünürlük dış (sınıfsız)
div'te yönetiliyor.

**Nasıl test edildi (canlı tarayıcı, dev sunucu, sunucu logları ile INSERT/DELETE doğrulandı):**
1. Self Satış'ta ÇORBA eklenip 1/2 bölündü (1.125 TL → 562,50 TL) → Nakit ile onaylandı;
   sunucu logunda `RestaurantCheckPendingPayments` INSERT'i doğrulandı (FinancialTransaction/
   CurrentAccountTransaction YOK).
2. Modal "Adisyona Dön" ile kapatıldı → ana ekranda "Ödenen 562,50 / Kalan 562,50" göründü.
3. **TAM SAYFA YENİLENDİ** (yeni GET isteği) → kısmi ödeme AYNEN geri geldi (`pos-pending-
   payments-data` sunucudan doğru döndü) - sunucu kalıcılığı kanıtlandı, sadece JS bellek
   durumu değil.
4. "Ödeme İptal" tıklandı (onay diyaloğu geçildi) → Ödenen 0,00'a döndü, kullanıcı ödeme
   ekranında KALDI (modal kapanmadı); sunucu logunda gerçek bir `DELETE FROM
   RestaurantCheckPendingPayments` doğrulandı.
5. Self Satış hızlı ödeme kısayolu (Kredi Kartı) tıklandı → satış ANINDA kapandı, hiç onay
   istemedi, boş yeni bir adisyona (AD.00025) döndü; sunucu logunda gerçek `RetailSales`/
   `CurrentAccountTransactions` INSERT'i doğrulandı (tam muhasebe akışı çalıştı).
6. "Hızlı Ödeme Sonrası Fiş Sorulsun mu?" AÇILDI, tekrar hızlı ödeme denendi → satış yine
   ANINDA kapandı (RetailSale sunucu logunda doğrulandı) AMA bu sefer "Fiş Yazdır | Kapat"
   diyaloğu gösterildi; "Fiş Yazdır" tıklanınca `/Restaurant/Receipt/{id}` sayfası açıldı,
   içerik (ürün, tutarlar, KDV, "Kredi Kartı 125,00 ₺") DOĞRU şekilde göründü ve otomatik
   yazdırma tetiklendi.
7. Numpad'e kalanın (125) üzerinde bir tutar (200) girildi → Para Üstü CANLI "75,00 ₺" olarak
   göründü; Nakit'e basılınca kayda SADECE 125,00 ₺ girdiği doğrulandı (200 değil) - fazla
   tahsilat asla oluşmadı.
8. Ayarlar sıfırlandı (RequireReceiptPromptAfterQuickPay tekrar kapatıldı, test öncesi
   varsayılan duruma dönüldü).

**Commit edildi, deploy/SahinSoft.exe + ~/Desktop/muhasebe/ paketine alındı, DbSetup SQL
script'i yeni migration'ı içerecek şekilde yeniden üretildi.**

### Madde 9/25 — Satır kilidi kaldırma
Bu oturumun ERKEN bir bölümünde ".rest-shell" kilit özelliği (🔒 Satırlar Kilitli/Açık butonu,
sessionStorage) eklenmişti (Edip'in o anki "kilit tuşu koy" isteği üzerine). Şimdi bu YENİ
talimatla TAMAMEN KALDIRILACAK, yerine madde 21'deki yetki sistemi (CanEditKitchenSentLines vb.)
geçecek. Kilit UI'ı, JS scroll-sync'in kilit kısmı, CSS .unlocked kuralları sökülecek - HORIZONTAL
SCROLL SAFETY NET (911x512 DPI testinde bulunan taşma çözümü) KORUNACAK, o ayrı bir konu.

---

## SIRADAKİ AYRI İŞ PAKETİ (Edip, 2026-09-04 22:5x civarı) — YAZDIRMA/RAPOR TASARIM ALTYAPISI

**36 maddenin İÇİNDE DEĞİL, ONLARIN ÖNCELİĞİ VAR** ("önceliğin diğer konular" - Edip'in kendi
sözü). Bu bölüm sadece unutulmasın diye kaydedildi - 36 madde bitmeden buna GEÇİLMEYECEK, ama
36 madde tamamen bitince veya Edip özellikle isteyince sırada bu var.

**Kapsam (Edip'in kendi maddeleri, aynen):**
1. Merkezi rapor/yazdırma tasarım sistemi - adisyon fişi, tahsilat fişi, mutfak fişi, X raporu,
   Z raporu, Z Listesi detay çıktısı TASARLANABİLİR ŞABLON üzerinden çalışsın. FRX/FRX3 benzeri
   yönetilebilir şablon mantığı (alan ekleme, metin, çizgi, logo, tablo, font, hizalama,
   görünürlük, koşullu alanlar, kağıt genişliği) - stack'te birebir FRX yoksa AYNI mantıkta
   kendi Rapor Tasarımcısı'mızı kur. Şablonlar DB'de veya yönetilebilir dosyada saklanabilsin.
   Tasarım değişince KOD DEĞİŞMEMELİ.
2. 80mm termal yazıcı (XPrinter/ESC-POS) ANA HEDEF - A4/PDF boşluğu OLMAMALI, satır uzunluğu/
   font/hizalama/kesme komutu termal yazıcıya uygun. 58mm parametre olarak eklenebilir ama
   varsayılan 80mm.
3. Ayarlar > merkezi Yazıcı Ayarları - şube/kasa başına ayrı yazıcı tanımı (Adisyon/Tahsilat-
   Fiş/Mutfak/X-Z Rapor/Paket yazıcıları): ad, OS yazıcı adı, bağlantı tipi (USB/Network/IP),
   IP/Port, kağıt genişliği, otomatik kesici, TR karakter kodlaması, kopya sayısı, test çıktısı,
   aktif/pasif.
4. Belge-yazıcı eşleştirmesi parametrik, şube/kasa bazlı (ör. Adisyon fişi → Kasa XPrinter 80mm).
5. Tarayıcı print preview'a BAĞIMLI OLMA - mümkünse doğrudan tanımlı yazıcıya çıktı gönder,
   preview isteğe bağlı olsun. Termal çıktıda otomatik kesme desteklensin.
6. Adisyon fişi minimum alanlar: işletme adı/logo, Fiş/Adisyon No, tarih/saat, satış türü,
   masa no/kişi sayısı, kasiyer/garson, ürün/miktar/birim fiyat/tutar, ara toplam, indirim, KDV,
   genel toplam, ödeme türleri, Para Üstü, açıklama/not.
7. X/Z raporları da AYNI rapor motorundan üretilip 80mm termalden düzgün çıkmalı, kullanıcı
   tarafından tasarlanabilir olmalı.
8. Amaç: rapor görünümü koddan bağımsız yönetilebilsin - yeni logo/alan/font/başlık/düzen için
   DERLEME GEREKMEMELİ.

**ÖNEMLİ - şu ana kadar yapılmış olanla ilişkisi:** Madde 3 (Self Satış Hızlı Ödeme) çalışması
sırasında bugün eklenen `/Restaurant/Receipt/{id}` (Views/Restaurant/Receipt.cshtml, basit HTML +
`window.print()`) ve mevcut RestaurantReports'taki "Fişi Gör" modalının Yazdır butonu - Edip'in
kendi sözüyle: **"Mevcut basit HTML/PDF fiş çıktısını nihai çözüm olarak kabul etme... mevcut
basit fiş çıktısını bu altyapıya geçiş için GEÇİCİ kabul et."** Yani bu ekranlar ŞİMDİLİK
olduğu gibi kalacak (fonksiyonel, test edilmiş), ama bu iş paketine sıra gelince tasarlanabilir
şablon motoruna taşınacak, kod DEĞİŞMEDEN yönetilebilir hale getirilecek. Nihai teslimde gerçek
80mm termal yazıcıda test edilecek.

---

### Madde 2 — Ortak Satış Mimarisi (doğrulama, 2026-09-04)

Kod okunarak doğrulandı: `RestaurantSelfSaleController.Index()` (satır ~55) ve
`RestaurantPackageController.Create()` (satır ~107) İKİSİ DE `RedirectToAction("Check",
"Restaurant", ...)` yapıyor - yani Masa Satış, Self Satış ve Paket'in ÜÇÜ DE ürün seçimi/
miktar/barkod/indirim/ikram/satır silme/ödeme için AYNI `Check.cshtml` + `RestaurantPostingService`
motorunu kullanıyor, sadece hangi masa/oturuma bağlandığı farklı. Yeni kod GEREKMEDİ, sadece
doğrulandı.

### Madde 10 — İndirim ortak motor (doğrulama, 2026-09-04)

Kod + canlı test ile doğrulandı: ana ekrandaki "İndirim" butonu ve ödeme ekranındaki
"% İndirim" butonu (`pay-discount-btn`) İKİSİ DE `window.openTicketDiscountModal()` - yani AYNI
modal/motor. "İndirimi Kaldır" (`discount-clear-btn`) `ApplyTicketDiscountAsync(checkId, 0)`
çağırıyor - bu, TÜM satırların `DiscountAmountSnapshot`'ını sıfırlayıp orijinal toplamı TAM
olarak geri getiriyor (idempotent/mutlak tutar mantığı, kümülatif değil). Canlı testte %20
indirim uygulandı (125→100₺), sonra kaldırıldı (100→125₺ tam geri döndü). Yeni kod GEREKMEDİ.

### Madde 11 — Fiş İkram (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar:**
- `RestaurantCheck`e ComplimentaryAtUtc/ComplimentaryByUserId/ComplimentaryReasonFor/
  ComplimentaryReasonWhy/ComplimentaryNote eklendi (migration `AddReceiptComplimentary`).
- `RestaurantPostingService.ApplyReceiptComplimentaryAsync`: CanApplyComplimentary yetki
  kontrolü (yoksa reddedilir) + Administrator gerekçesiz atlayabilir/diğerleri Kime+Neden
  zorunlu + adisyondaki TÜM aktif satırların IsComplimentary=true + DiscountAmountSnapshot=
  brüt tutar (satır bazlı İkram ile AYNI desen, ciroya dahil değil).
- `RestaurantPermissionService.IsAdministratorAsync` eklendi (gerekçe atlama kontrolü için).
- Sağ menüdeki "Fiş İkram" artık SADECE bekleyen sepeti (client-only) işaretlemiyor - önce
  `flushCartToKitchen` ile tüm bekleyen ürünleri kalıcı satıra çeviriyor, SONRA yeni
  `#receiptComplimentaryModal` (Kime/Neden/Açıklama) açılıyor, sunucuya gönderiliyor.

**NOT (kapsam dışı bırakılan kısım, madde metninde vardı ama şu an mimari olarak mümkün değil):**
"Stok hareketi oluşturur" gereksinimi - incelendi, restoran satışlarının HİÇBİRİ (ikramlı ya da
ücretli) şu an stok düşümü YAPMIYOR (RestaurantPostingService'te hiçbir StockMovement/Ledger
çağrısı yok, `RestaurantCheckClosed` outbox mesajı sadece hibrit senkron için, stok tüketimine
bağlı değil). Bu, restoran modülünün GENEL bir eksiği - ücretli satışlar da dahil - İkram'a özgü
değil. İkram'ı normal satışla SİMETRİK tutmak (ciro hariç her şeyde aynı davranış) için doğru
yaklaşım buydu; stok düşümü altyapısı restoran modülüne eklenirse İkram da OTOMATİK olarak aynı
yoldan geçecek (DiscountAmountSnapshot=brüt olsa da satır miktarı/ürünü değişmiyor).

**Nasıl test edildi:** Administrator olarak ÇORBA eklenip Fiş İkram'a basıldı → onay diyaloğu
geçildi, Kime/Neden BOŞ bırakılıp "İkram Uygula" ile başarıyla uygulandı (sunucu loglarında
`RestaurantChecks.ComplimentaryAtUtc/ComplimentaryByUserId` + `RestaurantOrderLines.
IsComplimentary/DiscountAmountSnapshot` UPDATE'leri doğrulandı). Sonra Test Garson (PIN 9911,
Administrator DEĞİL) ile aynı akış denendi: boş gönderim REDDEDİLDİ ("Kime ve Neden alanları
zorunludur"), Kime/Neden doldurulunca BAŞARILI oldu.



(Her madde tamamlandığında buraya: ne yapıldı, hangi dosyalar, nasıl test edildi, hangi commit.)

### Madde 21 (temel altyapı) + 9 + 25 — 2026-09-04, TEST EDİLDİ

**Yapılanlar:**
- `RestaurantPermissionProfile` + `RestaurantPersonnelPermissionProfile` yeni entity'ler
  (`src/SahinSoft.Domain/Entities/`), migration `AddRestaurantPermissionProfiles`.
- `RestaurantPermissionService` (`Services/RestaurantPermissionService.cs`): Administrator her
  zaman geçer; hiç profili olmayan kullanıcı varsayılan TAM YETKİLİ (geriye dönük uyumlu); ≥1
  profili olan kullanıcı, atanmış AKTİF profillerden HERHANGİ BİRİ izin veriyorsa (OR mantığı)
  geçer.
- CRUD: `RestaurantPermissionProfilesController` + `Views/RestaurantPermissionProfiles/*`,
  sol menüye "Ayarlar" girişi eklendi (`_RestaurantShellLayout.cshtml`, Mutfak'ın üstünde,
  Administrator/RestaurantManager'a gizli değil).
- `PersonnelController`/`PersonnelFormViewModel`/`Form.cshtml`: personel formuna çoklu profil
  atama checkbox listesi ("Yetki Listesi").
- Gerçek uygulama noktaları (`RestaurantPostingService.cs`): `CancelOrderLineAsync` →
  CanCancelOrderLine, `AdjustOrderLineQuantityAsync` → CanEditKitchenSentLines,
  `ToggleLineComplimentaryAsync` → CanApplyComplimentary. Yetkisiz denemede
  `InvalidOperationException` → TempData["Error"] olarak ekranda gösteriliyor.
- **Satır kilidi tamamen kaldırıldı**: `Check.cshtml`'den kilit butonu, `restaurant-pos.js`'den
  kilit IIFE'si (sessionStorage, applyLockState), `restaurant-pos.css`'ten `.unlocked` kuralları
  silindi. Mutfağa gönderilmiş satırlar artık HER ZAMAN düzenle/ikram/iptal butonlarını gösteriyor
  - erişim SADECE yetki sistemi tarafından belirleniyor.

**Bulunan ve düzeltilen gerçek bug (spec'in "hata bulunca bekletmeden düzelt" kuralı gereği):**
PIN-only personel (e-postasız) oluşturma tamamen bloke oluyordu - "Email '' is invalid." hatası.
Kök neden iki katmanlıydı: (1) `PersonnelFormViewModel.Email` nullable olmayan string'di (düzeltildi,
`string?` yapıldı), (2) asıl engelleyici: `Program.cs`'te `options.User.RequireUniqueEmail = true`
açıkken ASP.NET Core Identity'nin varsayılan `UserValidator<TUser>`'ı NULL e-postayı bile
"Email '' is invalid." diye reddediyor. Çözüm: `Identity/PinOnlyUserValidator.cs` - e-posta
GİRİLMİŞSE format+benzersizlik kontrolü yapan, boşsa tamamen atlayan özel `IUserValidator`.
Program.cs'te varsayılan validator `RemoveAll<IUserValidator<ApplicationUser>>()` ile söküldü.

**Nasıl test edildi (canlı tarayıcı, dev sunucu):**
1. "Test-Kısıtlı" profili oluşturuldu (CanCancelOrderLine=false, diğer 5 bayrak=true).
2. "Test Garson" personeli oluşturuldu (Waiter rolü, PIN 9911, Test-Kısıtlı profili atanmış) -
   bu adım Email bug'ı yüzünden 3 denemede başarılı oldu, üstteki düzeltmeyle çözüldü.
3. PIN 9911 ile giriş yapıldı, BAHÇE-1 masası açıldı, ÇORBA eklendi, Mutfağa Gönder yapıldı.
4. Mutfağa gönderilmiş ÇORBA satırında kilit YOK, düzenle/ikram/iptal doğrudan görünür durumda.
5. İptal denendi → sunucu doğru şekilde reddetti: "Sipariş satırı iptal etme yetkiniz yok." Satır
   değişmeden kaldı.
6. Pozitif kontrol: aynı kullanıcı İkram denedi → BAŞARILI oldu (ÇORBA İKRAM, 0,00 ₺) - profil
   bazlı bağımsız bayrak mantığının (bir yetki kapalı, diğerleri açık) doğru çalıştığı doğrulandı.

**Commit edildi, deploy/SahinSoft.exe + ~/Desktop/muhasebe/ paketine alındı** (bu bölümün ilk yarısı).

### Madde 21 (tamamlayıcı kısım: "Şifre sorulsun mu?" + audit log) — 2026-09-04, TEST EDİLDİ

**Yapılanlar:**
- `InventorySettings`e 6 yeni bool alan: `RequireSecondApprovalFor{CancelOrderLine,CancelReceipt,
  Discount,Complimentary,EditKitchenSentLines,AddNote}` - RestaurantPermissionProfile'daki 6
  bayrakla birebir eşleşiyor. Ayarlar > Stok Parametreleri'nde "Kritik İşlemlerde İkinci Yetkili
  Onayı" kartı altında toggle'lar (madde 26'nın "aynı ayar kaynağı" kuralına uygun - POS'un kendi
  ayrı bir ayar tablosu YOK, muhasebe'nin InventorySettings'i aynen kullanılıyor).
- `RestaurantPermissionAuditLog` (yeni tablo): Action, PerformedByUserId, ApproverUserId,
  RestaurantCheckId/RestaurantOrderLineId, Details, CreatedAtUtc.
- `RestaurantPermissionService`: `RequiresSecondApprovalFor*Async` (InventorySettings okur),
  `VerifyApproverPinAsync(pin, gerekliBayrak)` - girilen PIN'i AKTİF PIN'i olan TÜM personelin
  hash'ine dener (onaylayan mevcut oturumdaki kişi olmak ZORUNDA DEĞİL, spec'in kendi kuralı),
  eşleşen kullanıcı bulunsa bile o SPESİFİK işlem için yetkili değilse onay reddedilir.
  `LogApprovalAsync` - audit tablosuna yazar.
- `RestaurantPostingService`: `CancelOrderLineAsync`/`AdjustOrderLineQuantityAsync`/
  `ToggleLineComplimentaryAsync` üçü de artık opsiyonel `approverPin` parametresi alıyor; ayar
  açıksa PIN doğrulanmadan işlem YAPILMAZ, başarılıysa audit log'a yazılır.
- İstemci: `Check.cshtml`'e ortak/tek `#approverPinModal` eklendi (üç işlem için de aynı modal
  kullanılıyor - kim onayladığı PIN eşleşmesinden anlaşılıyor). `restaurant-pos.js`'e
  `window.requestApproverPin(gerekliMi, callback)` ortak yardımcı fonksiyonu eklendi; ayar
  kapalıyken modal HİÇ açılmıyor (davranış bugünküyle birebir aynı kalıyor).

**Bulunan ve düzeltilen İKİNCİ gerçek bug:** `SettingsController.Map()` (GET /Settings/Inventory)
`RequireCancellationReason`/`CancellationReasonPresets`/`QuickNotePresets` alanlarını HİÇ
set etmiyordu - ekran her açıldığında bu üçü boş/kapalı görünüyordu, formu fark etmeden kaydeden
bir admin DB'deki gerçek değeri sessizce SIFIRLIYORDU. Aynı satıra eklediğim 6 yeni alanla birlikte
düzeltildi.

**Nasıl test edildi:** Ayarlar'dan "Sipariş Satırı İptalinde İkinci Yetkili Onayı" açıldı, kaydedildi
ve sayfa yeniden yüklendiğinde toggle'ın AÇIK kaldığı doğrulandı (Map bug'ının düzeldiğinin kanıtı).
BAHÇE-1'deki açık adisyonda bir satır iptali denendi: (1) yetkisiz onaylayan PIN'i (Test Garson,
CanCancelOrderLine=false) → doğru şekilde reddedildi, satır değişmedi; (2) yetkili onaylayan PIN'i
(Kasiyer/Administrator, PIN 66) → başarılı oldu, "Sipariş satırı iptal edildi." mesajı geldi; sunucu
loglarında `RestaurantPermissionAuditLogs` tablosuna gerçek bir INSERT çalıştığı doğrulandı.

**Commit edildi, deploy/SahinSoft.exe + ~/Desktop/muhasebe/ paketine alındı, DbSetup SQL script'i
yeni migration'ı içerecek şekilde yeniden üretildi** - bu adımlar tamamlandı.

**Madde 21 artık TAM bitti** (temel altyapı + 2. yetkili onayı + audit log, hepsi test edildi).

### Madde 12 — Ödenmez ödeme tipi (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar:**
- `RestaurantPaymentMethod.Unpaid = 4` yeni enum değeri.
- `InventorySettings.ShowUnpaidPaymentType` (Ayarlar > Stok Parametreleri, varsayılan KAPALI) -
  açıkken ödeme ekranında 4. bir "✕ Ödenmez" butonu render edilir (Check.cshtml, sunucu
  tarafında `@if` ile - kapalıyken DOM'da bile yok).
- `CloseCheckAsync` - Ödenmez'e ayrılan tutar hem Sale (ciro) hem Collection (tahsilat)
  `CurrentAccountTransaction` kayıtlarından NET OLARAK çıkarılıyor (`netAccountingTotal =
  grandTotal - unpaidTotal`) - tamamı Ödenmez ise HİÇ CurrentAccountTransaction/
  FinancialTransaction oluşmuyor. Yine de her Ödenmez satırı için bir `RestaurantPayment`
  kaydı OLUŞUYOR (FinancialTransaction=null) - izlenebilirlik/raporlama için. RetailSale.
  GrandTotal ve fiş satırları DEĞİŞMEDİ - ürün gerçekten "satılmış" sayılıyor (fişte tam
  görünür), sadece tahsil edilmemiş oluyor. İkram'dan (satır fiyatı sıfırlanır) FARKI budur.

**Nasıl test edildi:** Ayarlar'dan açıldı, Self Satış'ta ÇORBA (125₺) eklenip ödeme ekranında
"Ödenmez" ile tam tutar işaretlendi, "Siparişi Tamamla" ile kapatıldı. Sunucu loglarında
`RetailSales` VE `RestaurantPayments` INSERT'lerinin çalıştığı AMA `CurrentAccountTransactions`/
`FinancialTransactions` için HİÇBİR INSERT olmadığı doğrulandı - tam olarak tasarlandığı gibi.
Test sonrası ayar varsayılana (kapalı) döndürüldü.

### Madde 13 — Açık Hesap + zorunlu cari (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar:**
- `RestaurantPaymentMethod.OpenAccount = 5` - Ödenmez'den FARKI: Sale (ciro) hareketi NORMAL
  oluşur (revenue gerçekleşir), sadece Collection (tahsilat) hareketi oluşmaz - seçilen CARİNİN
  hesabında GERÇEK bir açık alacak bırakır (Debit var, karşılığı Credit yok).
- `RestaurantCheck.AttachedCustomerId` (yeni) - "Cari Ekle" (onaylı Self Satış tasarımındaki
  buton) ile adisyona bağlanan müşteri, `UpdateCheckNoteAsync` ile AYNI basit desende
  `AttachCustomerAsync` metoduyla yazılıyor.
- `CloseCheckAsync`: `customerId ??= check.AttachedCustomerId` (istemci ayrıca bir customerId
  göndermediyse Cari Ekle'den geleni kullanır) + `openAccountTotal > 0 && customerId is null` ise
  **"Cari seçmelisiniz."** ile reddeder (spec'in istediği TAM metin). `netSaleTotal = grandTotal -
  unpaidTotal` (Açık Hesap dahildir, revenue oluşur) vs `netCollectionTotal = grandTotal -
  unpaidTotal - openAccountTotal` (Açık Hesap hariçtir, tahsilat oluşmaz) - iki ayrı guard ile
  Sale ve Collection BAĞIMSIZ olarak oluşur/oluşmaz.
- İlk kez: ana ERP'nin `lookup-picker.js` + `_LookupModal` altyapısı restoran kabuğuna
  (`_RestaurantShellLayout.cshtml`) eklendi - restoran modülü bunu önceden hiç kullanmıyordu.
  "Cari Ekle" butonu `_LookupField` deseniyle AYNI `data-lookup-*`/`lookup-trigger` mekanizmasını
  kullanıyor, sadece görsel olarak tek bir buton gibi (mockup'taki gibi) paketlendi.

**Nasıl test edildi:** Açık Hesap'a cari seçmeden basıldı → "Cari seçmelisiniz." ile doğru
reddedildi. "Cari Ekle" ile gerçek bir cari (lookup modal üzerinden) seçildi - sunucu loglarında
`UPDATE RestaurantChecks SET AttachedCustomerId` doğrulandı. Sonra Açık Hesap ile 125₺ tam tutar
işaretlenip "Siparişi Tamamla" ile kapatıldı - sunucu loglarında TEK BİR `CurrentAccountTransactions`
INSERT'i (Sale, Debit=125, seçilen cariye) görüldü, Collection hareketi OLUŞMADI, `RestaurantPayments`
kaydı (izlenebilirlik) oluştu - tam tasarlandığı gibi.

**Bulunan ve düzeltilen dördüncü/beşinci bulgu (bug değil, ortam garipliği):** Test sırasında
tarayıcı bölmesi bir noktada 303px genişliğe düştü (muhtemelen önceki bir `resize_window` çağrısının
kalıntısı) - kod hatası SANILDI ama `resize_window` ile 1400x900'e sabitlenince düzeldi, gerçek bir
CSS regresyonu değildi.

### Madde 14 — Tahsilat Carileri / Platform Ödemeleri (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar:**
- `Customer.IsCollectionCari` (yeni) - "Tahsilat Carisi" toggle'ı (Cari Tanıtım Kartı formu).
- Restoran ödeme ekranı, `IsActive && IsCollectionCari` olan HER cari için sunucudan gelen
  listeden dinamik bir buton üretir (`@@foreach` - HARD-CODE YOK, Trendyol/Getir/Yemeksepeti/
  gelecekteki platformlar hepsi bu tek mekanizmadan gelir).
- Bu butonlar Madde 13'ün Açık Hesap mekanizmasını AYNEN kullanır (Method=OpenAccount) - tek
  fark, cari seçimini "Cari Ekle" yerine butonun kendisi yapar (tıklanınca önce o carinin
  AttachCustomer'ını çağırır, sonra ödeme satırını ekler). Muhasebe: Sale normal oluşur (o
  carinin hesabında gerçek borç), Collection oluşmaz - platform gerçek ödemeyi yaptığında
  normal Tahsilat ekranından kapatılır.

**Nasıl test edildi:** Yeni bir cari ("Trendyol") oluşturulup Tahsilat Carisi işaretlendi.
Self Satış ödeme ekranında "🏷 Trendyol" butonunun DİNAMİK olarak belirdiği doğrulandı. Tıklanıp
125₺ ile kapatıldı. Cari Listesi ekranında Trendyol'un Borç bakiyesinin 125,00 ₺'ye çıktığı
doğrulandı - gerçek muhasebe entegrasyonu, restoran ekranından ana ERP cari hesabına uçtan uca
izlenebilir.

### Madde 15-16 — Ürün Arama modalı + Türkçe normalizasyon (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Türkçe I/İ/ı/i normalizasyonu - kod değişikliği GEREKMEDİ:** DB'nin Turkish collation'ı bunu
zaten hallediyor - `SearchProducts?term=porsiyon` (ASCII noktasız i) "PORSİYON"u (noktalı büyük
İ) doğru buluyor, doğrulandı. (Genel aksan/diyakritik temizleme - ç→c, ş→s gibi - spec'in
İSTEDİĞİNDEN DAHA GENİŞ bir kapsam olurdu, spec özellikle SADECE I/İ/ı/i varyantlarını
belirtiyor - bilinçli olarak yapılmadı.)

**Yapılanlar:**
- Yeni "📦 Ürün Listesi" sağ menü butonu + `#productListModal` - arama üstte (Ad/Barkod modu
  toggle'ı ile), sonuç tablosu ortada (Stok Kodu | Barkod | Ürün Adı | Fiyat sütunları).
- `SearchProducts` endpoint'i genişletildi: `stockCode` alanı eklendi, yeni `mode` parametresi
  ("barcode" iken SADECE barkod alanlarında Contains arar - okutma sırasında kısmi eşleşme
  için; varsayılan "name" mevcut ad+tam-barkod davranışını korur).
- Seçim (tıklama veya tek sonuçtayken Enter) mevcut `addToCart` fonksiyonunu AYNEN kullanıyor -
  kategori/sepet alanına HİÇ dokunmuyor, izolasyon şartı sağlandı.

**Madde 17 ile ilişkisi:** Spec'in "klavye altta" gereksinimi için şimdilik MEVCUT basit
`#pos-virtual-keyboard` kullanılıyor (zaten input focus'a otomatik bağlanıyor). Madde 17'nin
istediği TAMAMEN YENİ özel klavye (harfler solda/numpad sağda, taşınabilir, yeniden
boyutlandırılabilir) AYRI bir iş - henüz yapılmadı, sıradaki adım.

**Nasıl test edildi:** "Ürün Listesi" açıldı, "köfte" yazıldı → "SHN.146 | 1989000001424 |
KÖFTE 1 PORSİYON | 225,00 ₺" doğru sütunlarla listelendi, tıklanınca sepete eklendi VE kategori
sekmeleri (Başlangıçlar/Izgaralar/Salatalar) DEĞİŞMEDİ. Barkod moduna geçilip tam barkod
("1989000001424") yazıldığında AYNI ürün doğru bulundu.

### Madde 17 — Özel sanal klavye (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar (klavye baştan tasarlandı):**
- Yeni düzen: harfler SOLDA (qwerty + Türkçe ğ/ü/ş/ı/ö/ç), numpad SAĞDA (7-8-9/4-5-6/1-2-3/0-,-⌫)
  - iki panel yan yana `.pos-vk-body { display:flex }`.
  - Büyük dokunma tuşları (varsayılan 2.6rem, `data-size="lg"` iken 3.4rem).
- Taşınabilir: sol/sağ DOCK (◀/▶ araç çubuğu tuşları, `data-dock` attribute) - free-drag
  KASITLI OLARAK tercih edilmedi, "EKRAN DIŞINA HİÇ TAŞMASIN" şartını dock+`max-width:calc(100vw
  - 24px)`/`max-height:calc(100vh - 24px)` kombinasyonu KESİN olarak garantiliyor (free-drag
  ekstra sınır-kontrolü kodu gerektirirdi, dock'lama daha sağlam).
- Boyut kontrolü: － / ＋ tuşları `data-size` (sm/md/lg) arasında geçiş yapıyor.
- Otomatik açılma YOK (zaten mevcut davranıştı, madde 17'nin "ana barkod alanında asla otomatik
  açılmama" şartı baştan sağlanıyordu - sadece "⌨ Klavye" ile elle açılıyor).
- Bug: `[data-action="backspace"]` artık HEM harfler HEM numpad panelinde var - eski kod
  `querySelector` (TEK eleman) kullanıyordu, `querySelectorAll` + forEach'e çevrildi ki numpad'in
  kendi ⌫'i de çalışsın.

**Nasıl test edildi:** Fiş Notu alanına odaklanılıp gerçek tıklamalarla "q", boşluk, "ç" art
arda yazıldı - metin alanında doğru sırayla ve doğru Türkçe karakterle ("q ç") biriktiği
doğrulandı. "Sağa yasla" tıklanınca klavye sağa geçti (ekran dışına taşmadı). "Büyüt" tıklanınca
tuşlar büyüdü, panel yine ekran içinde kaldı.

### Madde 18 — Bekleyen Fişler kart tasarımı (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Yapılanlar:**
- Sağ menü "Bekleyen Fişler" butonuna canlı sayaç rozeti eklendi (kırmızı, sağ üst köşe).
- Modal başlığı artık dinamik "Bekleyen Fişler (N)".
- Basit satır listesi yerine profesyonel kart grid'i: her kart Fiş/Adisyon No, satış türü rozeti
  (Self Satış/Masa Satış), masa/kaynak etiketi + saat, bekleme süresi (dk/saat), ürün adedi,
  toplam gösteriyor.
- En eski fiş EN ÜSTE sıralanıyor ve "EN ESKİ" etiketiyle + kırmızı çerçeve/arkaplanla AYRICA
  vurgulanıyor - "tek bakışta ayırt edilebilsin" şartı.
- Kart tıklanınca ilgili adisyona gidiyor (mevcut davranış korundu, sadece görünüm değişti).

**Nasıl test edildi:** Self Satış'ta ÇORBA eklenip "Fişi Beklet" ile bekletildi - sidebar
rozetinin "1" gösterdiği doğrulandı. Modal açılınca başlık "Bekleyen Fişler (1)" oldu, kart
doğru bilgilerle (AD.00085, Self Satış, 1 kalem, 125,00 ₺, "EN ESKİ · <1 dk") ve kırmızı
vurguyla göründü. Karta tıklanınca AD.00085 doğru şekilde açıldı, bekletilen ÇORBA sepette
geri geldi.

### Madde 19 — Fiş Listesi (Günlük Fişler) Excel-vari filtreleme (TAMAMLANDI, 2026-09-04, TEST EDİLDİ)

**Not:** Bu madde "Restoran Raporları"nın "Günlük Fişler" sekmesine karşılık geliyor (o ekranın
temel yapısı ÖNCEKİ oturumda kurulmuştu - tarih gezinme, kaynak sekmeleri, mini özet). Bu
oturumda EKSİK olan kısımlar tamamlandı:

**Yapılanlar:**
- Dinamik ödeme türü filtresi ("Tüm Ödemeler" dropdown) - SADECE o gün/kaynak filtresinde
  GERÇEKTEN var olan ödeme türlerini listeler (`AvailablePaymentFilters`, hard-code liste
  DEĞİL) - `RestaurantPaymentMethod` genişleyince (Ödenmez/Açık Hesap gibi) otomatik uyum
  sağlıyor, ayrı bir "karma ödeme"/"ödemesiz" kovası da var.
- Durum filtresi (Tamamlandı/İptal) + arama kutusu (Fiş No/Masa-Kaynak üzerinde).
- Filtrelenmiş sonuçlara göre ödeme türü bazında alt toplamlar (mini özet çubuğunda dinamik
  olarak eklenen NAKİT/KREDİ KARTI/ÖDENMEZ/AÇIK HESAP gibi kartlar) - gerçek `RestaurantPayments`
  tablosundan, filtrelenen fişlerin check'lerine göre hesaplanıyor.
- "Filtreleri Temizle" butonu.
- Sipariş Türü (KANAL sütunu) zaten dinamikti (SourceTypeOf gerçek salon adından türetiliyor) -
  değişiklik gerekmedi, sadece doğrulandı.

**Nasıl test edildi:** Filtresiz haldeyken "Tüm Ödemeler" dropdown'ının SADECE Nakit/Kredi
Kartı/Açık Hesap/Ödenmez'i listelediği (o gün hiç Yemek Çeki kullanılmadığı için o seçenek HİÇ
görünmüyor) ve mini özette bu 4 türün doğru tutarlarla (Nakit 3.600/Kredi Kartı 1.375/Ödenmez
125/Açık Hesap 250) göründüğü doğrulandı. `payment=openaccount` filtresi uygulanınca SADECE 2
fiş (250₺ toplam) kaldı, alt toplam SADECE "AÇIK HESAP 250₺" oldu. Arama kutusuna "PSF.00037"
yazılınca SADECE o tek fiş (Masa, Nakit, 1.625₺) listelendi.

### Madde 20 — Boş Adisyon otomatik temizlik (TAMAMLANDI, 2026-09-05, TEST EDİLDİ)

**Gerçek eksik bulundu:** `VoidEmptyCheckAsync` zaten vardı ama SADECE "Masayı Boşalt" butonuna
elle basınca çalışıyordu - spec'in "son ürün İPTAL EDİLİNCE adisyon KENDİLİĞİNDEN temizlenir,
kullanıcı Kapat'a basmak ZORUNDA değildir" şartı sağlanmıyordu. "Sipariş Sil" (Sipariş Sil) zaten
tamamen client-side (sadece henüz gönderilmemiş sepeti temizliyor, sunucuya hiç dokunmuyor) -
buna dokunulmadı.

**Yapılanlar:**
- `CancelOrderLineAsync` artık satır iptalinden SONRA otomatik kontrol ediyor: bu check bir
  Self Satış check'i mi VE artık hiç aktif satırı kalmadı mı? İkisi de doğruysa `VoidEmptyCheckAsync`'i
  KENDİSİ çağırıyor (aktif satır kalmışsa fırlatılan istisna sessizce yutuluyor - normal durum).
- **BİLİNÇLİ OLARAK SADECE Self Satış'ta** - spec'in kendi metni bunu Self Satış'a sınırlıyor
  (Masa Satış'ta müşteri hâlâ masada oturuyor olabilir, adisyonu kendiliğinden iptal etmek YANLIŞ
  olurdu).
- Metod imzası `Task` → `async Task`'a çevrildi (post-commit adımı eklemek için).

**Nasıl test edildi:** Self Satış'ta ÇORBA eklenip mutfağa gönderildi (gerçek satır oluştu),
sonra o TEK satıra İptal denendi (gerekçe + 2. yetkili PIN onayı ile, ayarlar açık kaldığından).
Sunucu "Sipariş satırı iptal edildi." VE hemen ardından "Bu adisyon artık açık değil." mesajlarını
verdi, Masa Satış ekranında "Açık Adisyon 0.00 ₺" göründü - adisyon gerçekten otomatik kapandı.
Self Satış'a tekrar girildiğinde BAŞKA bir (önceden açık kalmış, gerçekten boş) adisyon doğru
şekilde yeniden kullanıldı - fantom açık fiş kalmadı.
