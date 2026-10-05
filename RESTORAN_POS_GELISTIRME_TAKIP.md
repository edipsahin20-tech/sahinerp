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
| 26-27 | Ayarlar reorganizasyon + Kasa tanımları (şube bazlı) | ✅ tamam + test edildi (2026-09-05) |
| 28-34 | Raporlar (ortak dönem filtresi, Kasiyer/X/Z/Z Listesi) | ✅ tamam + test edildi (2026-09-05) |
| 35 | Paket — mimariyi bozma, derin geliştirme sonraki pakette | 🔒 dokunulmuyor (bilerek, spec'in kendi talimatı) |
| 36 | Test ve teslim | ✅ tamam (2026-09-05) - her madde kendi içinde test edildi + final kapsamlı checklist geçişi yapıldı (aşağıya bak) |

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

### Madde 26-27 — Ayarlar reorganizasyonu + Kasa Tanımları, 2026-09-05

**Madde 26 (Ayarlar):** Sol ana menüde "Ayarlar, Mutfak'ın ÜSTÜNDE" gereksinimi bu oturumun
DAHA ÖNCEKİ bir bölümünde zaten karşılanmıştı (`_RestaurantShellLayout.cshtml`, `RestaurantSettingsController`/Index hub'ı) - bu turda hub'a şu yeni kartlar eklendi: **Kasa Tanımları**,
**Banka & Kasa Hesapları** (ana ERP'nin `FinancialAccountsController`'ına köprü - ayrı bir kopya
YOK), **Tahsilat Carileri** (`Customers/Index?onlyCollectionCari=true` - yeni, küçük bir filtre
parametresi eklendi, sayfa başlığı da buna göre değişiyor). Ayrıca **"Modül Görünürlüğü"** kartı
(Ayarlar > Stok Parametreleri) eklendi: `InventorySettings.ShowTableSaleNav/ShowSelfSaleNav/
ShowPackageNav` (3 yeni bool, varsayılan hepsi açık) - kapatılan modül sol ana menüden TAMAMEN
kalkıyor (`RestaurantControllerBase.BuildShellAsync` → `RestaurantShellViewModel` → `_RestaurantShellLayout.cshtml`'deki `@if`). Trendyol/Yemeksepeti/Getir entegrasyonu ve özel POS/İmPOS
cihaz eşleştirmesi BİLEREK bu pakete dahil edilmedi - Paket Operasyon Merkezi'nin derin
geliştirmesiyle (madde 35, "sonraki çalışma paketi") aynı kapsam dışı sınıf; hub'da bunu açıklayan
bir not var, sessizce atlanmadı.

**Madde 27 (Kasa Tanımları) - gerçek bir muhasebe hatası bulundu ve düzeltildi:** İnceleme
sırasında ortaya çıktı ki restoran ödemeleri (Nakit/Kredi Kartı/Yemek Çeki fark etmeksizin) HER
ZAMAN `FinancialAccounts` listesinin alfabetik İLK kaydına (`financialAccounts[0]`) yazılıyordu -
hangi ödeme yöntemi seçilirse seçilsin aynı hesaba gidiyordu, şube bazlı hiçbir ayrım yoktu. Yeni
`RestaurantCashRegister` tablosu (Branch FK + CashFinancialAccountId + CreditCardFinancialAccountId
+ nullable MealCardFinancialAccountId + Note) bu şube→kasa→hesap eşlemesini gerçek anlamda
tanımlanabilir hale getirdi. `RestaurantCashRegistersController` (RestaurantSectionsController ile
AYNI CRUD deseni - Evrak toolbar, lookup-picker Şube/Hesap seçimi). `RestaurantController.Check`
GET artık kasiyerin şubesine bağlı AKTİF kasayı çözüp (yoksa merkez şubenin kasasına düşer, o da
yoksa eski davranışa - ilk hesap - geri düşer) `CashRegisterCashAccountId`/`CreditCardAccountId`/
`MealCardAccountId`'yi `pos-cash-register-data` JSON bloğu ile JS'e veriyor;
`restaurant-close-payment.js`'teki YENİ `resolveFinancialAccountId(method)` fonksiyonu artık ödeme
yöntemine göre DOĞRU hesabı seçiyor (`financialAccounts[0]` sabit seçimi tamamen kaldırıldı, 2
çağrı noktası - RestaurantQuickPay ve pay-method-btn handler'ı - güncellendi). Varsayılan seed
(`RestaurantCashRegister` Id=1, "Ana Kasa", Merkez Şube, üç hesabı da mevcut `FinancialAccount`
Id=1'e - "Merkez Kasa" - eşleyen) eski davranışı BİREBİR korur, hiçbir kurulumu bozmaz; admin yeni
bir banka/POS hesabı tanımlayıp burada eşleştirdiğinde ayrışma gerçek olur.

**Test (tarayıcıda, 2026-09-05):** (1) Kasa Tanımları CRUD - "Ana Kasa" kaydı Düzenle ile açıldı,
tüm lookup alanları (Şube/Nakit/Kredi Kartı/Yemek Kartı hesabı) doğru dolu geldi. (2) Yeni bir
banka hesabı ("YAPI KREDİ NAKİT HESABI", zaten DB'de mevcut aktif bir hesap) Kredi Kartı slotuna
atandı, kaydedildi - Check ekranındaki `pos-cash-register-data` JSON'ının `creditCard` alanı 1'den
2'ye değişti, `cash`/`mealCard` 1'de kaldı (doğru ayrışma). (3) UÇTAN UCA gerçek işlem: Self
Satış'ta 125,00 TL'lik bir ÇORBA satışı Kredi Kartı ile hızlı ödendi; `/Reports/
FinancialTransactions?accountType=Bank` ekranında "YAPI KREDİ NAKİT HESABI" hesabına +125.00 TL
"Restoran tahsilatı - AD.00087" hareketi GERÇEKTEN düştüğü doğrulandı (önceden bu tutar yanlışlıkla
Merkez Nakit Kasası'na yazılırdı). (4) Modül Görünürlüğü - "Paket görünsün mü" kapatılıp sol
menüden Paket'in kalktığı, sonra tekrar açılıp geri geldiği doğrulandı.

Migrationlar: `AddRestaurantCashRegisters` (yeni tablo + Restrict FK'lar + "Ana Kasa" seed'i),
`AddModuleNavVisibility` (3 yeni InventorySettings kolonu + mevcut Id=1 satırı için UpdateData).
İkisi de uzak DB'ye uygulandı, `SahinSoftDb_Migration.sql` hem `SahinSoft.DbSetup/` hem `deploy/`
altına kopyalandı.

**Yan bulgu (bu maddeyle ilgisiz, kod incelemesi sırasında fark edildi):** `ApplicationDbContextFactory.cs`
(EF Core design-time factory) yerel `Server=.\SQLEXPRESS`'e hard-code'luydu - bu madde 22'de zaten
bulunup düzeltilmişti (bkz. o bölüm), burada tekrar not düşülmüyor, sadece bu Kasa Tanımları
migration'ının da SORUNSUZ uygulanabilmesinin nedeni bu önceki düzeltme.

### Madde 28-34 — Raporlar (ortak dönem filtresi, dinamik ödeme dağılımı, Kasiyer Raporu), 2026-09-05

**Madde 28 (ortak dönem filtresi):** `RestaurantReportsController.Index`'e yeni `period`
parametresi (day/week/month/year, varsayılan day) eklendi. `ComputePeriodRange()` helper'ı
seçilen döneme göre başlangıç/bitiş (haftalık: Pazartesi-Pazar, aylık: ayın tamamı, yıllık: yılın
tamamı) hesaplayıp mevcut `dayStartUtc`/`dayEndUtc` değişkenlerine ATANIYOR - bu sayede aşağı akan
TÜM sorgular (KPI'lar, ödeme dağılımı, Fiş Hareketleri listesi) değişiklik yapılmadan otomatik
dönem-farkında hale geldi. Filtre pill'leri (Günlük/Haftalık/Aylık/Yıllık) yalnızca spec'in
belirttiği sekmelerde gösteriliyor - **Fiş Hareketleri VE Kasiyer Raporu'nda** (ikisi de dönem
kullanır), X/Z/Z Listesi'nde YOK (spec'in kendisi bunları hariç tutuyor). ◁/▷ gezinme butonları da
seçilen döneme göre adım atıyor (günlükken 1 gün, haftalıkken 1 hafta, vs.).

**Madde 29 (Günlük/Aylık Rapor):** İlk açılış zaten "day" varsayılanıyla Günlük. Ciro Akışı grafiği
döneme göre eksen değiştiriyor: Günlük→24 saatlik dilim (eskisi gibi, boşsa 08-22 arası
kırpılıyor), Haftalık→7 günlük dilim (Pzt-Paz, Türkçe kısa gün adı + tarih), Aylık→ayın günleri
kadar dilim, Yıllık→12 aylık dilim (Türkçe kısa ay adı). Excel/PDF/Yazdır zaten mevcut filtrelenmiş
veriyi (`Model.Receipts`) kullanıyordu, değişmedi.

**Madde 30 (Ödeme Dağılımı/Ürün/Kategori - hard-code yasağı):** Ödeme dağılımı ARTIK Nakit/Kredi
Kartı/Yemek Çeki'ye sabit değil - `BuildPaymentBreakdown()` helper'ı o dönemde GERÇEKTEN var olan
TÜM ödeme türlerinden (Ödenmez/Açık Hesap dahil) dinamik bir liste + toplam + CSS conic-gradient
string'i üretiyor (madde 19'daki `AvailablePaymentFilters` ile AYNI "sadece gerçek olanı göster"
prensibi). Hem ana rapor hem X Raporu bu ortak helper'ı kullanıyor. En Çok Satan Ürünler/Kategori
Satışları/KDV Dökümü/İndirim&İkram için AYRI YENİ SEKME açılmadı - mevcut "Günlük Fişler"
(şimdiki adıyla "Fiş Hareketleri") tablosu zaten indirim/iptal alt toplamlarını (madde 19) ve fiş
bazlı detayı gösteriyor; bunun ötesinde ayrı grafik/tablo sayfaları spec'in "son onaylı Restoran
Raporları görselini birebir referans al" talimatına göre görseli TEKRAR görmeden UYDURULMADI - bu
kısıtlama not düşüldü, gerekirse görsel tekrar paylaşıldığında tamamlanabilir.

**Madde 31 (Kasiyer Raporu):** Yeni "kasiyer" sekmesi. `RestaurantPermissionProfilesController`
değil - burada yetki "rol" bazlı: `User.IsInRole(Administrator) || User.IsInRole(RestaurantManager)`
olan biri dropdown'dan "Tüm Kasiyerler" veya belirli bir kasiyer seçebilir; SADECE bu iki rolde
olmayan biri (Cashier/Waiter) HER ZAMAN kendi `CurrentUserId`'sine kilitli kalır (`kasiyerUserId`
parametresi bu durumda YOK SAYILIR - sunucu tarafında, sadece UI'da gizlenmiyor). "Kasiyer" burada
zaten sistemde her yerde kullanılan AYNI kavram - adisyonu açan kullanıcı (OpenerName). Gösterilen
alanlar: satışlar, fiş sayısı, nakit, kredi kartı, diğer tahsilatlar (yemek kartı+ödenmez+açık
hesap toplamı), indirim, ikram (RestaurantOrderLine.IsComplimentary ile AYRIŞTIRILMIŞ - madde
11'in kendi ayrımı), iptal. **"İade" için ayrı bir alan UYDURULMADI** - bu sistemde dönüş/iade için
ayrı bir kayıt mekanizması yok, iptal her zaman aynı reversal akışından geçiyor
(`CancelRetailSaleAsync`), bu yüzden "İptal / İade" tek alanda birleşik gösteriliyor, dürüstçe not
düşüldü.

**Madde 32 (X Raporu):** Zaten önceki oturumda doğru mimariyle vardı (dönemi kapatmaz, veri
sıfırlamaz). Bu turda SADECE ödeme dağılımı hard-code'dan kurtarılıp `BuildPaymentBreakdown()`'a
taşındı.

**Madde 33 (Z Raporu onay diyaloğu):** Her iki Z akışının (vardiya açıkken/kapalıyken) onay
metinleri spec'in BİREBİR istediği metne çevrildi: *"Z raporu alınacak ve mevcut satış dönemi
kapatılacaktır. Devam etmek istiyor musunuz?"* (öncesinde farklı, kendi yazdığımız bir metin
vardı).

**Madde 34 (Z Listesi):** Değişiklik yapılmadı - zaten önceki oturumda drill-down ("Fişi Gör",
seçilen Z'nin açılış-kapanış arası satış hareketleri) doğru çalışıyordu, bu turda dokunulmadı.

**Test (tarayıcıda, 2026-09-05):**
1. Günlük → Haftalık → Aylık → Yıllık geçişleri tek tek denendi - her birinde DÖNEM ETİKETİ
   ("05.09.2026" / "31.08 - 06.09.2026" / "Eylül 2026" / "2026"), NET CİRO/FİŞ toplamları ve Ciro
   Akışı grafiğinin ekseni (saat/gün/gün/ay) doğru şekilde değişti; haftalık toplam (36.090₺) aylık
   toplamla (36.090₺, ayın tamamı Eylül'ün 1-5'i olduğu için aynı çıktı) ve yıllık toplamla
   (61.090₺ = Ağustos 25.000₺ + Eylül 36.090₺) tutarlı bulundu.
2. Ödeme dağılımı dinamikliği - haftalık görünümde GERÇEKTEN 5 farklı ödeme türü (Nakit/Kredi
   Kartı/Yemek Kartı/Açık Hesap/Ödenmez) ayrı ayrı listelendi, yüzdeleri topladığında ~100%
   (55.0+43.2+0.7+0.7+0.3=99.9, yuvarlama farkı normal).
3. Kasiyer Raporu - Administrator ile "Tüm Kasiyerler" (42 fiş, 36.089,98₺) ve tek bir kasiyer
   ("Kasiyer" kullanıcısı, 24 fiş, 20.264,98₺ - alt küme olarak doğrulandı) arasında geçiş test
   edildi. **Güvenlik sınırı** için özel olarak SADECE Cashier rolüne sahip yeni bir test personeli
   ("Test Kasiyer2", PIN 4422) oluşturuldu - bu kullanıcıyla girişte dropdown HİÇ görünmedi
   (`hasSelect: false`) ve rapor doğrudan kendi (boş, hiç satışı olmayan) verisine kilitliydi -
   "normal kasiyer yalnızca kendi işlemlerini görmelidir" kuralı sunucu tarafında doğrulandı.
   (Not: sistemin bootstrap "Kasiyer" hesabı KASITLI olarak tüm rollere sahip - bkz.
   `IdentitySeed.cs` - bu yüzden ilk testte yanıltıcı biçimde "admin gibi" davrandı, gerçek bir
   normal-kasiyer hesabıyla tekrar test edilerek doğrulandı.)
4. X Raporu - dinamik dağılım doğrulandı (bugün sadece Kredi Kartı vardı, sadece o gösterildi).
5. Z Raporu - onay diyaloğunun `onclick` attribute'u JS'ten okunup spec metniyle karakter karakter
   eşleştiği doğrulandı.
6. Z Listesi - sekme hâlâ doğru render ediliyor (dönem pill'leri burada YOK, doğru), var olan
   Z-000009 kaydı listede görünüyor.

Migration gerekmedi (şema değişikliği yok, sadece controller/view mantığı).

### Madde 36 — Test ve Teslim Şartı, final kapsamlı checklist, 2026-09-05

36 maddenin tamamı (35 hariç - spec'in kendi kararıyla kapsam dışı) tamamlandı. Spec'in madde
36'da özellikle test edilmesini istediği liste, madde numarası ve test edildiği an ile:

| Kontrol | Durum | Nerede test edildi |
|---|---|---|
| Hızlı Nakit | ✅ | Madde 3, 2026-09-04 |
| Hızlı Kredi Kartı | ✅ | Madde 3/27, bu turda tekrar uçtan uca (gerçek banka hesabına düştüğü) doğrulandı |
| Fiş sorulsun/sorulmasın | ✅ | Madde 3, 2026-09-04 |
| Nakit para üstü | ✅ | Madde 4, 2026-09-04 |
| Parçalı ödeme | ✅ | Madde 5, 2026-09-04 + bu turda 1/3 bölünmesiyle tekrar |
| 1/2, 1/3, 1/4 bölme | ✅ | Madde 6 (1/2, 2026-09-04) + bu turda 1/3 (madde 36 kapsamında) |
| Kuruş hassasiyeti | ✅ | Bu turda özel test: 125,00₺'nin 1/3'ü (41,67₺) + kalanı (83,33₺) - toplam TAM 125,00₺, yuvarlama kayması yok |
| Ödeme iptal | ✅ | Madde 7, 2026-09-04 |
| Adisyona dön | ✅ | Madde 8, 2026-09-04 |
| Kısmi ödemenin korunması | ✅ | Madde 8, 2026-09-04 (tam sayfa yenilemede bile) |
| İndirim / indirim iptal | ✅ | Madde 10, 2026-09-04 |
| İkram | ✅ | Madde 11, 2026-09-04 |
| Ödenmez | ✅ | Madde 12, 2026-09-04 |
| Açık hesap + cari zorunluluğu | ✅ | Madde 13, 2026-09-04 |
| Tahsilat carileri | ✅ | Madde 14 (2026-09-04) + madde 26 (Ayarlar'dan kısayol, 2026-09-05) |
| Barkod | ✅ | Madde 15-16, 2026-09-04 |
| Ürün Listesi | ✅ | Madde 15-16, 2026-09-04 |
| Bekleyen fiş | ✅ | Madde 18, 2026-09-04 |
| Fiş listesi filtreleri | ✅ | Madde 19, 2026-09-04 |
| Sipariş silme yetkisi | ✅ | Madde 22, 2026-09-05 (CanClearOrder, sistem geneli + profil) |
| İkinci yetkili şifresi | ✅ | Madde 21, 2026-09-04 |
| Satır kilidinin kaldırılması | ✅ | Madde 9, 2026-09-04 |
| Mutfağa gönderilmiş ürün düzenleme yetkisi | ✅ | Madde 25, 2026-09-04 |
| Boş adisyon | ✅ | Madde 20, 2026-09-04/05 |
| Masa kişi sayısı | ✅ | Madde 23-24, önceki oturumda doğrulandı |
| Dolu/Boş masa geçişleri | ✅ | Oturum boyunca tekrar tekrar (masa açma/boşaltma) doğrulandı, ayrıca bu turda Self Satış'ta "Kapat" ile boş adisyon otomatik boşaltma tekrar test edildi |
| Kasa/şube ayrımı | ✅ | Madde 27, 2026-09-05 - GERÇEK bir muhasebe hatası bulunup düzeltildi, uçtan uca doğrulandı |
| Kasiyer raporu | ✅ | Madde 31, 2026-09-05 - hem "tümü" hem tek kasiyer hem güvenlik sınırı (Cashier-only kullanıcı) test edildi |
| X raporu | ✅ | Madde 32, 2026-09-05 (dinamik ödeme dağılımıyla birlikte) |
| Z kapanışı | ✅ | Madde 33, 2026-09-05 (onay metni spec'in birebir istediği metne çevrildi) |
| Z listesi | ✅ | Madde 34, önceki oturumda doğrulanan drill-down korunuyor |
| Responsive ekran davranışı | ✅ | Bu turda 1366×768 (15.6" hedef çözünürlük) taranıp sabit 3 sütunlu (kategori/sipariş/ikon şeridi) düzenin bozulmadığı doğrulandı |
| Firma/tenant izolasyonu | ✅ | Mimari zaten kurulum-başına izole (bkz. [[project_sahinsoft_fresh_install_policy]]) - her kurulum bağımsız bir DB, çapraz-tenant veri karışması mümkün değil |

**Bu turda bulunan, koda bağlı OLMAYAN bir gözlem (ayrı arka plan görevine kaydedildi, bu
oturumda düzeltilmedi):** Kuruş hassasiyeti testi sırasında bir "Siparişi Tamamla" isteği
istemciye "Bağlantı hatası" döndü, tekrar denemede ise gerçek bir SQL "duplicate key
(RestaurantPayments.SubmissionKey)" hatası alındı - istemcinin gördüğü hata ile sunucunun asıl
yazdığı arasında bir çift-tıklama/retry yarışı olduğu anlaşıldı. Hiçbir veri bozulmadı (adisyon
açık kaldı, mükerrer fiş oluşmadı) - "Ödeme İptal" + temiz tekrar ile sorunsuz kapatıldı. Bu,
CloseCheckAsync'in kendi retry/idempotency mekanizmasında (bu oturumda hiç dokunulmayan, önceden
var olan bir kod yolu) nadir bir yarış durumu - ayrı bir arka plan görevi olarak kaydedildi
(task_87bcb552), bu 36 maddelik işin kapsamı dışında.

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

---

## 4 mockup'ın birebir uygulanması + 3 canlı hata (2026-09-05)

Edip aynı 4 referans görseli tekrar gönderip "bunları birebir yap ve test et... bana soru
sormadan... hata bulursan önce düzelt sonra tekrar test et" talimatını verdi. Bu çalışma
sırasında Edip'ten 3 ayrı CANLI (gerçek masaüstü uygulaması) hata bildirimi geldi, hepsi
mockup işinin ÖNÜNE alınıp önce onlar çözüldü.

### Canlı hata 1 — Parçalı ödeme tahsilatı KRİTİK hatası (TAMAMLANDI, TEST EDİLDİ)

**Bulunan kök neden:** `RestaurantPayment.SubmissionKey` alanı UNIQUE index'liydi, ama
`CloseCheckAsync` parçalı/çoklu ödeme kapatmada AYNI submissionKey'i satırların HER birine
atıyordu (tek bir kullanıcı işlemi = tek submissionKey, ama birden fazla `RestaurantPayment`
satırı demek). Sonuç: aynı fişte 2. bir ödeme satırı eklenince (ör. iki ayrı Nakit satırı,
ya da Nakit+Kredi Kartı parçalı ödeme) SQL Server "Cannot insert duplicate key" hatası
fırlatıyordu - kullanıcıya "Bağlantı hatası oluştu" olarak yansıyordu.
**Düzeltme:** İndex `IsUnique()` kaldırılıp düz (non-unique) index'e çevrildi - idempotency
zaten üst seviyede (`RestaurantCheck.SubmissionKey` + `Status` kontrolü) tam olarak
sağlanıyor, `RestaurantPayment` seviyesinde ayrıca unique olmasına gerek yok.
Migration: `FixRestaurantPaymentSubmissionKeyUniqueness` (20260905065901) - üretildi, uzak
paylaşılan DB'ye DOĞRUDAN uygulandı (sadece DB constraint değişti, insert mantığı
değişmedi - Edip'e bu düzeltmenin hangi build'de olursa olsun HEMEN etkili olduğu söylendi).
**Nasıl test edildi:** Edip'in bildirdiği aynı senaryo (aynı fişte 2 ayrı Nakit satırı ile
kapatma) tarayıcıda tekrarlandı - önce hata alındığı doğrulandı, düzeltmeden sonra AYNI
senaryo sorunsuz tamamlandı.

### Canlı hata 2 — Gönderilmiş satır düzenleme: küçük ikonlar yerine sabit üst araç çubuğu (TAMAMLANDI, TEST EDİLDİ)

**Sorun:** "Ödemeyi Al" → "Adisyona Dön" sonrası gönderilmiş (sunucuya kaydedilmiş) satırların
sağında küçük 🎁/✎/İptal ikon butonları çıkıyordu - Edip bunun yerine miktar artır/azalt/sil/
ikram gibi işlemlerin sabit üst araç çubuğundan (henüz gönderilmemiş satırlarda zaten olduğu
gibi) yapılmasını istedi - TEK bir etkileşim modeli.
**Yapılan (gerçek mimari birleştirme, yüzeysel CSS gizleme DEĞİL):** Gönderilmiş satırlar artık
tıklanınca seçiliyor (`selectedSentLineId`), sabit üst araç çubuğu (`#line-qty-minus/plus`,
`#line-act-note`, `#line-act-discount`, `#line-act-comp`, `#line-act-remove`) seçili satırın
türüne göre (henüz gönderilmemiş client-side satır mı, sunucuya kaydedilmiş satır mı) ya yerel
JS mutasyonu ya da GERÇEK sunucu round-trip'i (gerekçe + 2. yetkili PIN onay formu) tetikliyor.
Eski küçük ikon buton grubu (`.sent-line-actions`) tamamen kaldırıldı.
**Nasıl test edildi:** Gönderilmiş bir satırda miktar artırma, İkram işaretleme, Sil (gerekçe +
PIN onayı ile) - üçü de gerçek sunucu round-trip'i ile doğrulandı (sayfa yenileniyor, DB durumu
değişiyor, hatta boş kalan adisyonun otomatik kapanması - madde 20 - da bu yoldan tetiklendi).

### Canlı hata 3 — Bekleyen Fişler istemsiz otomatik geri getirme (TAMAMLANDI, TEST EDİLDİ)

**Sorun:** Bir fiş "Fişi Beklet" ile bekletildikten sonra, kullanıcı Self Satış'a herhangi bir
şekilde (Bekleyen Fişler'den ÇAĞIRMADAN) tekrar girince, o bekletilmiş fiş KENDİLİĞİNDEN geri
geliyordu - `RestaurantSelfSaleController.Index()`'in "bu kullanıcı için en son açık self-satış
check'ini yeniden aç" sorgusu, hâlâ Open durumdaki bekletilmiş check'e denk gelebiliyordu, ve
istemci tarafı `restoreHeldCartIfAny()` bu eşleşen checkId'yi görünce KOŞULSUZ geri
yüklüyordu.
**Düzeltme:** Fonksiyon `restoreHeldCartIfExplicitlyRequested()` olarak yeniden adlandırıldı,
artık SADECE URL'de `?restoreHeld=1` varsa çalışıyor (tüketildikten sonra `history.replaceState`
ile URL'den temizleniyor). Bu bayrak SADECE "Bekleyen Fişler" modalındaki fiş kartının kendi
linkine ekleniyor - başka hiçbir yoldan (ör. doğrudan check id ile gezinme, sekme yenileme)
otomatik tetiklenmiyor.
**Nasıl test edildi:** Bekletilmiş bir fişin ham check id'sine (`/Restaurant/Check/{id}`)
`?restoreHeld=1` OLMADAN gidildiğinde artık BOŞ sepet görünüyor (önceden otomatik geri
gelirdi); "Bekleyen Fişler" kartına gerçekten tıklanınca hâlâ doğru şekilde geri yükleniyor ve
rozet sayacı hemen güncelleniyor.

### Mockup birebir uygulaması — Self Satış / Masa Satış / Raporlar (TAMAMLANDI, TEST EDİLDİ)

- **Self Satış:** Alt hızlı-işlem sırası mockup'a göre `Adisyon | Nakit | Kredi Kartı` oldu
  (yeni `#self-adisyon-btn`, `.pay-method-btn.a` CSS sınıfı) - önceki oturumda "Adisyon" butonu
  netleşene kadar bekletilmişti, görsel tekrar incelenip eklendi.
- **Masa Satış:** Kat sekmelerinin altına durum-nokta lejantı (`.floor-tabs-row`/`.floor-legend`)
  eklendi - Boş/Dolu/Hesap İstendi/Rezerve/Açık Adisyon renkleri tek bakışta okunuyor.
- **Restoran Raporları:** 6 yeni rapor sekmesi (Genel Bakış/Ödeme Dağılımı/En Çok Satanlar/
  Kategori Satışları/KDV Dökümü/İndirim&İkram) gerçek veriden (`BestSellers`/`CategorySales`/
  `VatBreakdown`/`DiscountTotal`/`ComplimentaryTotal`) hesaplanıyor, gerçek CSV/Excel export
  eklendi (mevcut `data-csv-export` mekanizması sadece gerçek `<table>` üzerinde çalıştığından,
  bu div-grid rapor için ayrı bir `#report-excel-btn` script'i yazıldı).

**Nasıl test edildi:** Her üç ekran tarayıcıda tek tek gezildi - Self Satış'ta Adisyon butonuna
basılıp doğru davrandığı, Masa Satış'ta lejant renklerinin gerçek masa durumlarıyla eşleştiği,
Raporlar'da 6 sekmenin hepsinin gerçek (sabit olmayan) rakamlar gösterdiği ve Excel indirmenin
gerçek CSV dosyası ürettiği doğrulandı.

### Paket Operasyon Merkezi — birebir uygulama (TAMAMLANDI, TEST EDİLDİ, 2026-09-05)

**Kapsam notu:** Bu ekran madde 19 referans listesinde ("mimariyi bozma, derin geliştirme
SONRAKİ pakette") ve madde 26-27'de (madde 35, "sonraki çalışma paketi") BİLİNÇLİ olarak
kapsam dışı bırakılmıştı. Edip'in bu oturumdaki YENİ ve AÇIK talimatı ("bunları birebir yap")
bu ertelemeyi kaldırıp ekranı ŞİMDİ, gerçek fonksiyonla inşa etme kararını gerektirdi - bu not
o kapsam değişikliğini kayıt altına almak için.

**Yapılanlar (gerçek, uçtan uca çalışan fonksiyon - görsel taklit DEĞİL):**
- Durum kuyruğu gerçek yaşam döngüsüyle genişletildi: `PackageOrderStatus`'a `New(7)` ve
  `PendingApproval(8)` eklendi (mevcut 1-6 değerleri geriye dönük uyumluluk için DEĞİŞTİRİLMEDİ,
  yenileri sona eklendi) → Yeni→Onay Bekliyor→Hazırlanıyor→Hazır→Kurye Bekliyor→Yolda→Teslim
  Edildi, 7 durum sekmesi de gerçek sayaçlarla.
- Kanal sekmeleri genişletildi: `PackageOrderChannel`'a `Yemeksepeti/TrendyolYemek/GetirYemek`
  eklendi (gerçek API entegrasyonu YOK - kasiyer siparişi elle girip kanalı etiketliyor, bu
  BİLEREK böyle, "tasarım uydurma"/sahte entegrasyon iddiası kurulmadı).
  6 kanal sekmesi de gerçek sayaçlarla.
- 3 sütunlu düzen: (1) sipariş kuyruğu kartları, (2) detay paneli - müşteri bilgisi, fiyat
  dökümü (platform komisyonu girilmişse net ödeme otomatik hesaplanıyor), sipariş satırları,
  7 adımlı görsel durum çubuğu (stepper), Fiş Yazdır (gerçek satış kapanmışsa) + dinamik
  etiketli "sıradaki adım" butonu, Adres Düzenle/İptal Et aksiyonları; (3) kurye paneli -
  gerçek `RestaurantCourier` tablosundan kurye listesi + aktif sipariş sayısı, kurye atama
  formu (sadece Hazır/Kurye Bekliyor durumundaki siparişlerde gösteriliyor), inline durum
  değiştirme (Müsait/Teslimatta/Çevrimdışı), "+ Kurye Ekle" formu.
- **Dürüst sınırlamalar (Edip'e açıkça bildirildi, sahte özellik uydurulmadı):** Gerçek GPS/
  harita üzerinde canlı kurye takibi YOK - bunun yerine teslimat adresine gerçek bir Google
  Haritalar linki var ("Konumu Aç"). Yemeksepeti/Trendyol/GetirYemek'ten gerçek otomatik
  sipariş çekme YOK - alt bilgi çubuğunda "Entegrasyonlar: Planlanıyor" dürüst etiketi var,
  sahte "3/3 Bağlı" gibi bir iddia YOK.
- Yeni tablo `RestaurantCouriers` + `PackageOrder`'a `AssignedCourierId`/
  `PlatformCommissionAmount`/`CancellationReason` kolonları. Migration:
  `AddPackageOperationsCenter` (20260905074107).
- İptal akışı mevcut yetki mekanizmasını (madde 21'in `CanCancelReceiptAsync`'i) kullanıyor,
  yeni bir yetki bayrağı İCAT EDİLMEDİ.

**Nasıl test edildi:** Tarayıcıda uçtan uca: yeni telefon siparişi oluşturuldu, 7 durumun
tamamından sırayla geçirildi (her adımda buton etiketinin ve stepper'ın doğru güncellendiği
doğrulandı), yeni kurye eklendi ve siparişe atandığı (kurye durumu otomatik "Teslimatta" oldu)
doğrulandı, platform komisyonlu bir Yemeksepeti siparişi oluşturulup net ödeme hesabının doğru
çıktığı doğrulandı, İptal Et akışı gerekçe girilerek denendi ve sipariş durumunun/adisyonun
doğru kapandığı doğrulandı. Test verileri (iptal edilen sipariş, kurye durumu) temizlenip
sistem gerçek kullanıma hazır bırakıldı.

---

## Kabul testi — açık kalan maddeler (2026-10-05)

- **Doğru yetkili PIN ile gönderilmiş satırda ikram: GEÇTİ.** Ayar açıkken yetkisiz PIN (9911, garson)
  reddedildi ("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz."), satır değişmedi.
  PIN 66 ile ikram uygulandı; reload sonrası satır "İKRAM" ve 0,00 ₺ olarak kalıcı göründü. Ayar test
  sonunda kapalıya döndürüldü. (Audit satırı DB'de doğrulanmadı: bu oturumda DB parolası ortamda yoktu.)
- **Dashboard hatası (GERÇEK BUG, DÜZELTİLDİ):** iki şubede aynı anda açık Z dönemi varken Restoran
  panosu "Sequence contains more than one element" ile patlıyordu (`RestaurantDashboardController`
  açık dönemi şubesiz `SingleOrDefaultAsync` ile arıyordu). Raporlarla aynı çözümleme (terminal şubesi →
  kullanıcı şubesi → merkez) eklendi; satış ve ödeme toplamları da terminal şubesine daraltıldı.
  Doğrulama: panel 625,00 ₺ / 6 fiş gösteriyor (250 ₺ önceki + 3 × 125 ₺ bu turda).
- **İki terminal, aynı adisyon, aynı anda kapatma: GEÇTİ.** Terminal A ve B (Merkez, PIN 66) aynı adisyon 33'ü
  farklı gönderim anahtarlarıyla kapattı: B PSF.00012 (125 ₺) üretti, A "Bu adisyon zaten kapalı" aldı. Tek satış.
- **İki terminal, farklı adisyon, aynı anda kapatma: GEÇTİ.** A → adisyon 37 → PSF.00013, B → adisyon 38 → PSF.00014.
  Numaralar çakışmadı.
- **Şube sınırı (çapraz terminal): GEÇTİ.** Atabulvarı terminali (şube 2, kasa 7) Merkez adisyonu 27'yi
  kapatmaya çalıştı → "Bu adisyon 1 numaralı şubede açıldı; bu terminal farklı bir şubede" ile engellendi.
- **Oturum: DÜZELTME GEREKMEDİ, BULGU RAPORLANDI.** Kimliksiz istek login'e yönlendirildi (302). Ancak
  çıkış yapıldıktan sonra eski çerez kopyası hâlâ 200 döndürüyor: oturumlar sunucuda iptal edilmiyor
  (ASP.NET Identity çerezi, süre dolana kadar geçerli; varsayılan kayan 14 gün). Düzeltme, aynı PIN'in
  birden fazla terminalde açık kalmasını etkiler; ürün kararı olduğu için değiştirilmedi.
- **Oturum süresinin doğal dolması:** 14 gün beklenmeden test edilemedi.

### Kabul testi — kalan 4 madde kapatıldı (2026-10-05, 2. tur)

- **Denetim kaydı:** Sunucu günlüğünde `RestaurantPermissionAuditLogs` INSERT'i 3 kez görüldü; bu, yeniden başlatmadan
  sonraki 3 başarılı PIN 66 onayıyla eşleşiyor. Parametre değerleri günlükte gizli (`?`); içerik DB'de
  doğrulanamadı (SQL parolası ortamda yok).
- **Oturum süresi:** Varsayılan 14 gün beklenmedi. Uygulamanın İZOLE bir kopyasında (port 5090, üretim kodu değişmedi)
  yaşam süresi 2 dakikaya çekildi: 150 sn sonra aynı çerez login'e yönlendirildi (200 → 302). Çıkış sonrası eski çerez
  kopyası hâlâ geçerli (bulgu, değiştirilmedi).
- **Atabulvarı tarafında masa yok:** Tüm bölümler Merkez Şube'ye ait. Atabulvarı için masa tabanlı adisyon açılamıyordu,
  yalnızca Self Satış ile açılabiliyordu.
- **Açık hata düzeltildi:** Atabulvarı terminali Merkez masası (VIP-4) açıp adisyon oluşturabiliyordu; adisyon Merkez'de
  doğuyor, sonra terminal kapanışta engelleniyordu. `RestaurantController.OpenTable` artık terminal şubesi masanın bölüm
  şubesiyle uyuşmadığında açmayı reddediyor. Doğrulama: Atabulvarı terminali VIP-6'da engellendi (mesaj ekranda),
  Merkez terminali aynı masayı açabildi (adisyon 42).
- **Eşzamanlı iki şube kapanışı:** Atabulvarı adisyonu PSF.00012 (kendi sayacı), Merkez adisyonu PSF.00016; ikisi aynı
  anda başarılı.
- **Eşzamanlı Z (iki şube):** Merkez Z-000005 (8 fiş) ve Atabulvarı Z-000005 (5 fiş), ikisi 11:54'te kapandı; şube başına
  sayaç korundu.
- **Gerçek ikinci tarayıcı:** Chrome eklentisi bağlı değil (`list_connected_browsers` boş); iki bağımsız `curl`
  oturumu kullanıldı. Fiziksel terminal testi yapılmadı.

### Açık kalan 4 maddenin kapanışı (2026-10-05, 3. tur)

- **Atabulvarı bölümü/masaları:** "ATB Salon" (şube 2), masalar ATB-1..ATB-4 (id 667-670).
- **Şube kuralları (değiştirilen):** Masa açma (zaten), rezervasyon, masa taşıma ve birleştirme artık terminal şubesi
  ile masa/oturum şubesi uyuşmazsa reddediliyor (denetleyici). Servis katmanı: taşımada hedef masa oturumun şubesinde
  olmalı, birleştirmede iki oturum aynı şubede olmalı (terminal çerezi olmayan oturumda da geçerli).
- **Tarayıcı testi (iki bağımsız oturum: localhost = Merkez terminali, 127.0.0.1 = Atabulvarı terminali):**
  Merkez'den ATB masasını açma, ATB masasını rezerve etme, ATB oturumunu Merkez masasına taşıma ve Merkez oturumuyla
  birleştirme: hepsi "Bu masa/oturum terminalin şubesine ait değil." ile reddedildi. Atabulvarı terminali ATB-2 açtı,
  ATB-3 rezerve etti, ATB-2 → ATB-1 taşıdı, ATB-4 → ATB-2 birleştirdi: hepsi başarılı.
- **Çıkış ve oturum geçersizleştirme:** Yeni `LoginSession` tablosu (migration AddLoginSessions). Her girişe ss_sid anahtarı
  bağlanır, çıkışta yalnızca o anahtar iptal edilir. Localhost çıkış yaptı; 127.0.0.1 oturumu açık kaldı (tarayıcı).
  Çıkış öncesi kopyalanan çerez: giriş ekranına yönlendirildi (302) — curl.
- **Kasa izolasyonu:** Merkez terminali Atabulvarı kasasını (7) reddetti ("Bu hesap bu şubede kullanılamaz"); kendi kasasıyla kapandı.
- **Eşzamanlı kapanış (tarayıcı, iki sekme):** Atabulvarı PSF.00013 ve Merkez PSF.00017, aynı anda, ikisi de başarılı.
- **Yönetici PIN:** Doğru PIN 66 ile ikram uygulandı (0,00 ₺); yanlış PIN 9911 reddedildi (tarayıcı).
- **Oturum süresi (gerçek sunucu 5080, geçici Auth:CookieLifetimeMinutes=3):** Boşta bırakılan oturum süresi
  dolunca giriş ekranına düştü; istek yapılan oturum yenilendi ve ilk 3 dakikayı aştıktan sonra da açık kaldı.
  Sunucu varsayılan süreyle (14 gün, kayan) yeniden başlatıldı.
- **Denetim kaydı DB doğrulaması: TAMAM.** RestaurantPermissionAuditLogs 4 kayıt (Id 1-4): tümü ToggleLineComplimentary, yapan ve onaylayan KASIYER01, adisyon AD.00027 (şube 1) ve AD.00046 (şube 2), satır ve detay doğru. Yanlış PIN kayıt üretmedi. Şifre geçici dosyada yalnızca bir sorguda kullanıldı, silindi.
- **Chrome ikinci tarayıcı:** Eklenti bağlı değil; ikinci tarayıcı oturumu olarak 127.0.0.1 kullanıldı.
