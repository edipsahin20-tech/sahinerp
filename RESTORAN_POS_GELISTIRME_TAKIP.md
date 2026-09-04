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
| 2 | Ortak Satış Mimarisi | ⏳ değerlendiriliyor |
| 3 | Self Satış Hızlı Ödeme + fiş sorulsun mu parametresi | ⏳ |
| 4 | Ödeme Al ekranı (Toplam/Ödenen/Kalan/Para Üstü) | ⏳ |
| 5 | Parçalı Ödeme | 🔶 kısmen mevcut, doğrulanacak |
| 6 | Tutarı Bölme (1/2,1/3..) | 🔶 kısmen mevcut, doğrulanacak |
| 7 | Ödeme İptal | ⏳ |
| 8 | Adisyona Dön (kilitlenmeden, kısmi ödeme korunur) | ⏳ |
| 9 | Satır Kilidi TAMAMEN KALDIRILACAK | ✅ tamam + test edildi (2026-09-04) |
| 10 | İndirim ortak motor | 🔶 kısmen mevcut (ApplyTicketDiscountAsync) |
| 11 | Fiş İkram (parametrik sebep, ciroya dahil değil) | ⏳ |
| 12 | Ödenmez ödeme tipi | ⏳ yeni |
| 13 | Açık Hesap + zorunlu cari | ⏳ |
| 14 | Tahsilat Carileri / platform ödemeleri | ⏳ yeni |
| 15-16 | Ürün Arama modal + Türkçe normalize | ⏳ |
| 17 | Özel sanal klavye | ⏳ yeni |
| 18 | Bekleyen Fişler kart tasarımı | ⏳ |
| 19 | Fiş Listesi Excel-vari filtrelenebilir | ⏳ |
| 20 | Boş Adisyon otomatik temizlik | 🔶 kısmen mevcut (VoidEmptyCheckAsync) |
| 21 | Yetki Mimarisi (profil, çoklu atama, kritik işlem) | 🔶 temel altyapı tamam + test edildi; "Şifre sorulsun mu" 2. yetkili onayı + audit log HENÜZ YOK |
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

### Madde 9/25 — Satır kilidi kaldırma
Bu oturumun ERKEN bir bölümünde ".rest-shell" kilit özelliği (🔒 Satırlar Kilitli/Açık butonu,
sessionStorage) eklenmişti (Edip'in o anki "kilit tuşu koy" isteği üzerine). Şimdi bu YENİ
talimatla TAMAMEN KALDIRILACAK, yerine madde 21'deki yetki sistemi (CanEditKitchenSentLines vb.)
geçecek. Kilit UI'ı, JS scroll-sync'in kilit kısmı, CSS .unlocked kuralları sökülecek - HORIZONTAL
SCROLL SAFETY NET (911x512 DPI testinde bulunan taşma çözümü) KORUNACAK, o ayrı bir konu.

---

## Detaylı madde notları (ilerledikçe güncellenecek)

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

**Henüz yapılmadı (madde 21'in geri kalanı):** "Şifre sorulsun mu?" - ikinci yetkili onayı
(mevcut oturumdaki kişi olmak zorunda olmayan bir PIN/şifre istemi) + `RestaurantPermissionAuditLog`
denetim kaydı. Bu, madde 21'i TAM bitirmek için gereken son parça, henüz başlanmadı.

**Commit edilmedi, deploy/muhasebe paketine alınmadı** - bu adımlar sıradaki iş.
