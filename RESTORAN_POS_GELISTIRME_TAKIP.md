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
| 12 | Ödenmez ödeme tipi | ⏳ yeni |
| 13 | Açık Hesap + zorunlu cari | ⏳ |
| 14 | Tahsilat Carileri / platform ödemeleri | ⏳ yeni |
| 15-16 | Ürün Arama modal + Türkçe normalize | ⏳ |
| 17 | Özel sanal klavye | ⏳ yeni |
| 18 | Bekleyen Fişler kart tasarımı | ⏳ |
| 19 | Fiş Listesi Excel-vari filtrelenebilir | ⏳ |
| 20 | Boş Adisyon otomatik temizlik | 🔶 kısmen mevcut (VoidEmptyCheckAsync) |
| 21 | Yetki Mimarisi (profil, çoklu atama, kritik işlem + 2. yetkili şifresi + audit log) | ✅ tamam + test edildi (2026-09-04) |
| 22 | Sağ İşlem Menüsü parametrik/yetki kontrollü | ⏳ |
| 23-24 | Masa Satış tasarım + kişi sayısı sorulsun mu | 🔶 "kişi sayısı sorulsun mu" akışı ilk açılışta zaten çalışıyor (doğrulandı), tasarım karşılaştırması henüz yapılmadı |
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
