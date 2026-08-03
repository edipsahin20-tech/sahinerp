# ŞahinSoft Restoran Modülü — Clean-Room Geliştirme Planı

**Durum: Faz 1 (şema + migration) hazırlandı, onay bekliyor. Migration `AddRestaurantModulePhase1Schema`
adıyla oluşturuldu ve `dotnet build` ile 0 hata doğrulandı — ancak canlı veritabanına HENÜZ
UYGULANMADI, deploy/commit/push yapılmadı. Bkz. §11 (genel onay + 5 karar alındı) ve ayrı
"Faz 1 Migration Raporu" (sohbette iletildi).**

## Clean-Room Kuralı

Bu modül DINOSOFTDB'nin (veya başka bir üçüncü taraf sistemin) kopyası **değildir**. Aşağıdaki hiçbir şey DINOSOFTDB'den alınmadı:

- Tablo adları, kolon adları
- SQL sorguları, stored procedure mantığı
- Ekran tasarımı, akış sırası
- İş kuralı isimlendirmesi

DINOSOFTDB yalnızca **ayrı ve salt-okunur** bir "ciro raporu" bağlantısı için kullanılıyor (bkz. `~/Desktop/DinosoftRaporPortali` — SahinSoft'un dışında, tamamen ayrı bir çözüm, ayrı veritabanı, kendi kimlik doğrulaması). Bu iki iş birbirine hiçbir noktada bağlı değil; DINOSOFTDB'yi incelememin tek amacı "böyle bir sistemde neler olması gerekir" konusunda genel farkındalık kazanmaktı (bkz. oturum içi analiz notları), tasarım kararlarının hiçbiri oradan kopyalanmadı — her tablo, her ilişki, aşağıda kendi gerekçesiyle sıfırdan tasarlandı.

**Neden bu tablolar var, kısaca:**

| Tablo | Neden var |
|---|---|
| RestaurantSections | Fiziksel salon/bölge gruplaması (Bahçe, Üst Kat vs.) |
| RestaurantTables | Fiziksel masa tanımı — durumu KENDİSİ tutmaz, aktif oturumdan hesaplanır |
| RestaurantTableSessions | Masanın açılıştan kapanışa yaşam döngüsü — "aynı masada iki aktif oturum olmaz" kuralının DB'de garantilendiği yer |
| RestaurantChecks | Bir oturumun hesabı/adisyonu — toplamlar, ödeme durumu |
| RestaurantOrders | Aynı adisyona farklı zamanlarda gönderilen her "sipariş turu" |
| RestaurantOrderLines | Sipariş kalemi — snapshot alanlarıyla (fiyat/KDV/reçete sürümü donmuş kalır) |
| RestaurantOrderLineModifiers | Kalem üstü ekstra/opsiyon, kendi fiyat snapshotuyla |
| ProductPortions | Bir menü ürününün porsiyon/beden seçenekleri |
| ProductRecipeHeaders | Reçetenin versiyonlu başlığı — "reçete değişince eski satış bozulmasın" kuralının temeli |
| ProductRecipeLines | Reçetedeki her hammadde satırı, miktar+fire |
| KitchenStations | Mutfak istasyonu — yazıcı/ekran adı BURADA tutulur, satırda değil |
| KitchenTickets | Bir siparişin, ilgili istasyona giden fişi |
| KitchenTicketLines | Fişin hangi sipariş kalemlerini kapsadığı |
| RestaurantPayments | Parçalı ödeme satırı (nakit/kart/yemek kartı) |
| RestaurantCashShifts | Kasiyer vardiyası — açılış/kapanış, kasa mutabakatı |

**Mimari not (netleştirme):** Önceki oturumlarda konuşulan "ayrı satellite POS uygulaması + merkez↔şube API senkronizasyonu" mimarisi hâlâ nihai hedef, ama bu belgedeki 15 tablo doğrudan SahinSoft.Web'in kendi `ApplicationDbContext`'i içinde, aynı süreçte, mevcut servisleri (DocumentNumberGeneratorService, PostingService deseni) doğrudan çağıracak şekilde tasarlandı — çünkü bu oturumdaki talimatınız bunu net biçimde istiyor ("Controller içinde doğrudan... yazma, yalnızca RestaurantPostingService kullanılmalı" gibi kurallar ancak aynı süreç/aynı DB'de anlamlı). Şubeler arası senkron/API katmanı, bu temel sağlamlaştıktan sonra gelecek bir sonraki faz olarak düşünülmeli. Bu tutarsızlık fark edilmemiş olabilir diye burada açıkça belirtiyorum — yanlış anlaşılmışsa söyleyin.

---

## 1. Mevcut ŞahinSoft Yapısıyla Çakışma Analizi

Aşağıdaki hiçbiri yeniden oluşturulmuyor, doğrudan kullanılıyor:

| Mevcut yapı | Restoran modülünde kullanımı |
|---|---|
| `Branch` | `RestaurantSection.BranchId`, `KitchenStation.BranchId`, `RestaurantCashShift.BranchId` |
| `Product` | Menü ürünü olarak doğrudan kullanılır — zaten `TrackStock`, `ShowAsShortcut`, `ShowInMobile`, `ShowInOnlineOrder`, `KitchenPrinterName`, `LoyaltyPoints` alanları var (2026-08-01'de restoran modülü için eklenmiş, şimdiye dek kullanılmıyordu). Yeni bir "MenuItem" tablosu YOK. |
| `Warehouse` | Reçete hammadde düşümünün hangi depodan yapılacağı (`ProductRecipeHeader.WarehouseId`) |
| `Customer` | Opsiyonel — adisyona müşteri bağlanabilir (örn. kurumsal fatura isteyen), ama walk-in siparişte zorunlu DEĞİL |
| `FinancialAccount` | `RestaurantPayment.FinancialAccountId`, `RestaurantCashShift.FinancialAccountId` |
| `Invoice` + `DocumentNumberGeneratorService` | Müşteri baştan kurumsal fatura isterse doğrudan mevcut Satış Faturası akışı kullanılır — yeni bir fatura mantığı YAZILMAZ |
| `StockMovement` | Reçeteye göre hammadde düşümü buraya yazılır, mevcut `ReversalOfId` alanı iptal için kullanılır |
| `CurrentAccountTransaction` / `FinancialTransaction` | Yalnızca kurumsal fatura veya cari bağlı adisyon durumunda dokunulur; peşin walk-in satışta cari hareketi oluşmaz |
| `AuditLog` | Hiçbir ek kod gerekmez — `EntityBase`'den türeyen her yeni tablo otomatik denetleniyor (`ApplicationDbContext.SaveChangesAsync` override'ı zaten generic) |
| `SubmissionKey`, `RowVersion` | Yeni tablolarda aynı desende (`Guid? SubmissionKey`, `EntityBase.RowVersion`) |
| `DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync` | `RestaurantPostingService`'in tüm Approve/Cancel benzeri işlemlerinde aynen kullanılır |
| `PostingService` + ters kayıt deseni | `RestaurantPostingService`, `PaymentReceiptPostingService`'in transaction/reversal şeklini birebir izler |

**Yeni eklenecek küçük, additive bir alan (15 tabloya dahil değil):** `Product.DefaultKitchenStationId` (nullable FK → `KitchenStations`) — bir ürünün varsayılan mutfak istasyonu. Mevcut `Product.KitchenPrinterName` alanına dokunulmuyor, o alan kalır (belki ileride kaldırılır ama bu plan kapsamında değil).

**Roller:** `AppRoles`'e `RestaurantManager`, `Cashier`, `Waiter`, `Kitchen` eklendi (bkz. §11 Karar 3) — `Administrator` her işleme yetkili olmaya devam ediyor.

---

## 2. 15 Çekirdek Tablo — Alanlar ve İlişkiler

Tümü `EntityBase`'den türer (`Id`, `RecordId`, `CreatedAtUtc`, `UpdatedAtUtc`, `RowVersion` otomatik gelir).

### RestaurantSections
| Alan | Tip | Not |
|---|---|---|
| BranchId | int (FK→Branch) | Restrict |
| Name | string(100) | |
| DisplayOrder | int | |
| IsActive | bool | |

### RestaurantTables
| Alan | Tip | Not |
|---|---|---|
| RestaurantSectionId | int (FK) | Restrict |
| Name | string(50) | Masa adı/no |
| Capacity | int | Kişi kapasitesi |
| PosX, PosY | decimal? | Salon planı görsel konumu (opsiyonel) |
| IsActive | bool | |

*Kasıtlı olarak YOK: `CurrentOrderId`, `CurrentSessionId`, kalıcı `Status`. Masanın "dolu/boş" durumu her zaman `RestaurantTableSessions`'ta `Status = Open` olan bir kayıt var mı diye sorgulanarak hesaplanır.*

### RestaurantTableSessions
| Alan | Tip | Not |
|---|---|---|
| RestaurantTableId | int (FK) | Restrict |
| Status | enum (Open, Closed) | |
| OpenedAtUtc | DateTime | |
| OpenedByUserId | string(450) | |
| GuestCount | int | |
| WaiterUserId | string(450)? | |
| ClosedAtUtc | DateTime? | |
| ClosedByUserId | string(450)? | |
| SubmissionKey | Guid? | |

**Unique index:** `(RestaurantTableId) WHERE Status = 'Open'` — aynı masada iki aktif oturum olamaz (DB seviyesinde garanti, uygulama kodu bunu bypass edemez).

### RestaurantChecks (Adisyon)
| Alan | Tip | Not |
|---|---|---|
| RestaurantTableSessionId | int (FK) | Restrict |
| CheckNumber | string(30) | DocumentNumberGeneratorService ile üretilir |
| Status | enum (Open, Closed, Cancelled) | |
| OpenedAtUtc, ClosedAtUtc | DateTime, DateTime? | |
| SubtotalAmount, DiscountAmount, ServiceChargeAmount, TaxAmount, GrandTotal | decimal(18,2) | Kapanışta sunucuda yeniden hesaplanır, istemciden gelen değere güvenilmez |
| LinkedInvoiceId | int? (FK→Invoice) | Kurumsal fatura istenirse |
| LinkedRetailSaleId | int? (FK→RetailSale) | Fatura istenmezse — bkz. §7, Karar 1 (kesinleşti: ayrı `RetailSale`/`RetailSaleLine` tablo ailesi) |
| CancelledByUserId, CancelledAtUtc, CancellationReason | string?, DateTime?, string? | Kapalı adisyon iptali için |
| SubmissionKey | Guid? | |

**Unique index:** `(CheckNumber)`, `(CreatedByUserId, SubmissionKey) WHERE SubmissionKey IS NOT NULL`.

### RestaurantOrders
| Alan | Tip | Not |
|---|---|---|
| RestaurantCheckId | int (FK) | Restrict |
| OrderedAtUtc | DateTime | |
| OrderedByUserId | string(450) | Garson |
| SubmissionKey | Guid? | Aynı "gönder" tıklamasının iki kez sipariş oluşturmaması için |

### RestaurantOrderLines
| Alan | Tip | Not |
|---|---|---|
| RestaurantOrderId | int (FK) | Restrict |
| ProductId | int (FK→Product) | Restrict |
| ProductPortionId | int? (FK→ProductPortions) | Restrict |
| Quantity | decimal(18,3) | |
| UnitPriceSnapshot, TaxRateSnapshot, DiscountAmountSnapshot | decimal | Sipariş anında donar |
| ProductNameSnapshot, PortionNameSnapshot | string | Ürün adı sonradan değişse bile eski sipariş bozulmaz |
| RecipeVersionUsed | int? | Kapanışta hangi reçete versiyonunun kullanıldığı |
| KitchenNote | string(500)? | |
| Status | enum (Ordered, Preparing, Ready, Served, Cancelled) | |
| CancelledByUserId, CancelledAtUtc, CancellationReason | string?, DateTime?, string? | Mutfağa gönderim sonrası iptalde zorunlu |

### RestaurantOrderLineModifiers
| Alan | Tip | Not |
|---|---|---|
| RestaurantOrderLineId | int (FK) | Restrict |
| NameSnapshot | string(150) | |
| PriceSnapshot | decimal | |
| Quantity | decimal(18,3) | |

### ProductPortions
| Alan | Tip | Not |
|---|---|---|
| ProductId | int (FK→Product) | Restrict |
| Name | string(50) | Küçük/Büyük/Normal vb. |
| PriceOverride | decimal? | Null ise `Product.SalePrice` kullanılır |
| IsDefault | bool | |
| DisplayOrder | int | |
| IsActive | bool | |

### ProductRecipeHeaders
| Alan | Tip | Not |
|---|---|---|
| ProductId | int (FK→Product) | Restrict |
| ProductPortionId | int? (FK→ProductPortions) | Null = porsiyonsuz ürün |
| BranchId | int? (FK→Branch) | Null = tüm şubeler |
| WarehouseId | int? (FK→Warehouse) | Varsayılan düşüm deposu |
| Version | int | Artan sürüm no |
| ValidFromUtc | DateTime | |
| ValidToUtc | DateTime? | Null = hâlâ aktif |
| YieldQuantity | decimal(18,3) | Üretim miktarı (örn. "1 tabak") |
| IsActive | bool | |

**Unique index:** `(ProductId, ProductPortionId, BranchId) WHERE ValidToUtc IS NULL` — aynı ürün/porsiyon/şube kombinasyonu için aynı anda yalnızca 1 aktif versiyon.

### ProductRecipeLines
| Alan | Tip | Not |
|---|---|---|
| ProductRecipeHeaderId | int (FK) | Restrict |
| IngredientProductId | int (FK→Product) | Restrict — hammadde de bir Product kaydıdır |
| UnitOfMeasureId | int? (FK→UnitOfMeasure) | |
| Quantity | decimal(18,3) | Kullanılan miktar |
| WastagePercent | decimal(5,2) | Fire oranı |

### KitchenStations
| Alan | Tip | Not |
|---|---|---|
| BranchId | int (FK→Branch) | Restrict |
| Name | string(100) | Izgara, Bar, Soğuk Mutfak vb. |
| PrinterName | string(150)? | Gerçek yazıcı/ekran adı BURADA — sipariş satırında metin olarak YAZILMAZ |
| DisplayOrder | int | |
| IsActive | bool | |

### KitchenTickets
| Alan | Tip | Not |
|---|---|---|
| RestaurantOrderId | int (FK) | Restrict |
| KitchenStationId | int (FK) | Restrict |
| TicketNumber | string(30)? | Ekran/yazıcı gösterimi |
| Status | enum (Sent, InProgress, Ready, Served) | |
| SentAtUtc | DateTime | |
| SubmissionKey | Guid? | |

**Unique index:** `(RestaurantOrderId, KitchenStationId)` — aynı sipariş+istasyon için mükerrer fiş oluşmaz.

### KitchenTicketLines
| Alan | Tip | Not |
|---|---|---|
| KitchenTicketId | int (FK) | Cascade (KitchenTicket'a ait) |
| RestaurantOrderLineId | int (FK→RestaurantOrderLines) | Restrict |
| Status | enum (Sent, InProgress, Ready, Served, Cancelled) | **Kendi bağımsız durumu** — bkz. Karar 4 |

**Karar 4 sonrası düzeltme (§11):** Orijinal taslakta `(RestaurantOrderLineId)` üzerinde tekil bir index ve "durum `RestaurantOrderLine.Status`'tan okunur" kuralı vardı. Onayınızla bu TERSİNE çevrildi: aynı sipariş kalemi farklı istasyonlara veya tekrar mutfağa gönderilebildiği için `RestaurantOrderLineId` üzerindeki tekillik kaldırıldı; bunun yerine **Unique index:** `(KitchenTicketId, RestaurantOrderLineId)` — aynı FİŞTE aynı satır yalnızca 1 kez, ama farklı fişlerde tekrar edebilir. `KitchenTicketLine.Status` artık kendi bağımsız alanı; `RestaurantOrderLine.Status` bundan hesaplanan bir önbellektir (Faz 2'de `RestaurantPostingService` tarafından güncellenir).

### RestaurantPayments
| Alan | Tip | Not |
|---|---|---|
| RestaurantCheckId | int (FK) | Restrict |
| PaymentMethod | enum (Cash, CreditCard, MealCard) | |
| Amount | decimal(18,2) | |
| FinancialAccountId | int (FK→FinancialAccount) | Restrict |
| FinancialTransactionId | int? (FK→FinancialTransaction) | Kapanışta doldurulur |
| IsReversal | bool | |
| ReversalOfId | int? (self FK) | |
| PaidAtUtc | DateTime | |
| SubmissionKey | Guid? | |

### RestaurantCashShifts
| Alan | Tip | Not |
|---|---|---|
| BranchId | int (FK→Branch) | Restrict |
| FinancialAccountId | int (FK→FinancialAccount) | Restrict |
| CashierUserId | string(450) | |
| Status | enum (Open, Closed) | |
| OpenedAtUtc | DateTime | |
| OpeningBalance | decimal(18,2) | |
| ClosedAtUtc | DateTime? | |
| ClosingBalanceExpected | decimal(18,2)? | Sistem hesaplar |
| ClosingBalanceCounted | decimal(18,2)? | Kasiyer girer |
| SubmissionKey | Guid? | |

**Unique index:** `(FinancialAccountId) WHERE Status = 'Open'` — aynı kasada iki açık vardiya olamaz.

---

## 3. Temel İlişkiler (özet)

```
Branch → RestaurantSection → RestaurantTable → RestaurantTableSession (1 aktif)
RestaurantTableSession → RestaurantCheck (1..n)
RestaurantCheck → RestaurantOrder (1..n) → RestaurantOrderLine (1..n) → RestaurantOrderLineModifier (0..n)
Product → ProductPortion (0..n)
Product/ProductPortion → ProductRecipeHeader (versiyonlu) → ProductRecipeLine (1..n, → Product hammadde)
RestaurantOrder → KitchenTicket (istasyon başına 1) → KitchenTicketLine (→ RestaurantOrderLine)
RestaurantCheck → RestaurantPayment (1..n)
Branch + FinancialAccount + Kasiyer → RestaurantCashShift
```

---

## 4. Durum Geçişleri

**RestaurantTableSession:** `Open → Closed` (yalnızca `RestaurantPostingService.CloseCheckAsync` tamamlandığında).

**RestaurantCheck:** `Open → Closed` (ödeme tamamlanınca) · `Open → Cancelled` (hiç ödeme yokken) · `Closed → Cancelled` (yetkili + neden zorunlu, ters kayıtla).

**RestaurantOrderLine:** `Ordered → Preparing → Ready → Served` · herhangi bir noktadan `→ Cancelled` (mutfağa gönderim SONRASI iptalde yetki + neden zorunlu; gönderim ÖNCESİ serbestçe silinebilir/düzenlenebilir).

**KitchenTicket:** `Sent → InProgress → Ready → Served`.

**RestaurantCashShift:** `Open → Closed`.

---

## 5. Unique / Index / RowVersion / SubmissionKey Kuralları

| Kural | Nasıl garanti ediliyor |
|---|---|
| Aynı masada iki aktif oturum olmaz | `RestaurantTableSessions` unique filtered index `(RestaurantTableId) WHERE Status='Open'` |
| Aynı adisyon iki kere kapanmaz | `RestaurantCheck.Status` kontrolü + `RowVersion` optimistic concurrency + `ExecuteWithConcurrencyRetryAsync` |
| Aynı ödeme iki kere yazılmaz | `RestaurantPayments.SubmissionKey` unique filtered index (`CreatedByUserId, SubmissionKey`) |
| Aynı mutfak siparişi iki kere basılmaz | `KitchenTickets` unique index `(RestaurantOrderId, KitchenStationId)` + `SubmissionKey` |
| Aynı kasada iki açık vardiya olmaz | `RestaurantCashShifts` unique filtered index `(FinancialAccountId) WHERE Status='Open'` |
| Çift tıklamayla mükerrer adisyon kapanışı | `RestaurantChecks.SubmissionKey` + `RowVersion` + `ExecuteWithConcurrencyRetryAsync` |
| Eşzamanlı 2 kullanıcı aynı reçeteyi güncellerse çakışma | `ProductRecipeHeaders.RowVersion` |

---

## 6. RestaurantPostingService — Kapanış ve İptal Akışı

Yalnızca bu servis kullanılır; hiçbir controller doğrudan stok/cari/kasa hareketi yazmaz, hiçbir yerde `Status` elle `Approved`/`Paid` yapılmaz.

### `CloseCheckAsync(checkId, paymentLines, submissionKey, userId)`

`PaymentReceiptPostingService` ile aynı iskelet:

```
DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
    dbContext.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        // 1. Adisyonu, masa oturumunu, satırları ve ödemeleri RowVersion ile TAZE yeniden yükle.
        // 2. Zaten Closed mı / bu SubmissionKey ile zaten işlenmiş mi kontrol et — öyleyse aynı sonuca dön.
        // 3. KDV, indirim, servis bedeli, genel toplamı SUNUCUDA yeniden hesapla (istemci değerine güvenme).
        // 4. RetailSaleDocument veya (kurumsal istekse) Invoice oluştur — DocumentNumberGeneratorService.GenerateWithinTransactionAsync.
        // 5. Her ödeme satırı için FinancialTransaction yaz (nakit/kart/yemek kartı → seçilen FinancialAccount).
        // 6. Aktif ProductRecipeHeader'a göre hammaddeleri ilgili depodan StockMovement ile düş
        //    (ReversalOfId deseniyle iptalde geri alınabilir).
        // 7. RestaurantCheck.Status = Closed, RestaurantTableSession.Status = Closed, masa boşalır
        //    (masa "boş" olur çünkü artık Open bir session'ı yok — ayrı bir "masa durumu" alanı YOK).
        // 8. SaveChangesAsync + CommitAsync — TEK commit.
        return true;
    }), ct);
```

Adım 1-7 arasında herhangi biri hata verirse `SaveChangesAsync`/`CommitAsync` hiç çağrılmaz → belge, ödeme, stok, masa, adisyon değişikliklerinin TAMAMI rollback olur (mevcut `IsolationLevel.Serializable` + transaction deseni bunu doğal olarak sağlar, `PaymentReceiptPostingService.ApproveCoreAsync`'te kanıtlanmış).

### `CancelCheckAsync(checkId, reason, userId)` — kapalı/ödenmiş adisyon iptali

`PaymentReceiptPostingService.CancelWithinTransactionAsync` deseninin birebir aynısı:
- Ödenmiş adisyon **hard-delete edilmez**.
- Her `RestaurantPayment` için `FinancialTransaction` reversal (Debit/Credit ters, `ReversalOfId`).
- Kurumsal fatura kesilmişse mevcut `InvoiceCancellationOrchestrationService` deseni devreye girer (ya da doğrudan `InvoicePostingService.CancelAsync`).
- Her düşülen hammadde `StockMovement` için ters kayıt (`ReversalOfId`), stok geri eklenir.
- `RestaurantCheck.Status = Cancelled`, `CancelledByUserId/AtUtc/Reason` doldurulur.
- Çift iptal: `Status == Cancelled` ise `InvalidOperationException` (mevcut `NegotiableInstrumentPostingService.CancelAsync`'teki "zaten iptal edilmiş" deseniyle aynı).

### `CancelOrderLineAsync(orderLineId, reason, userId)` — mutfağa gönderim sonrası satır iptali

Kendi küçük transaction'ı: `RestaurantOrderLine.Status != Cancelled` kontrolü, ilgili `KitchenTicketLine` varsa günceller, yetki kontrolü (bkz. §11 Karar 3), `CancellationReason` zorunlu.

---

## 7. Dahili Perakende Belge ile YN ÖKC Mali Fiş Ayrımı

**Seçenek A — Mevcut `Invoice`'a `RetailSale` türü eklemek:** `InvoiceType` enum'una üçüncü bir değer eklenir. Şema değişikliği küçük (additive enum), ama `Invoice` tablosu B2B kredi/vade mantığına göre tasarlanmış (`CreditDay`, `InvoicePaymentSchedule`, zengin vergi/tevkifat alanları) — restoran POS'un anlık, genellikle müşterisiz (walk-in), vadesiz satışları için bu ağır yapı gereksiz karmaşıklık taşır. Daha önemlisi: bu, dahili kaydın Fatura listelerinde/raporlarında GERÇEK faturaymış gibi görünmesi riskini taşır — sizin açıkça istemediğiniz şey ("resmî mali fişmiş gibi gösterme").

**Seçenek B — Ayrı `RestaurantRetailSaleDocument` tablosu (önerim):** Invoice'tan tamamen bağımsız, yalnızca restoran modülünün ürettiği hafif bir "dahili satış kaydı". Hiçbir zaman Fatura listesinde görünmez — karışma riski yapısal olarak sıfır. Gelecekteki mali alanlar (ÖKC seri no, mali fiş no, Z raporu no, `FiscalizationStatus`, ÖKC işlem kimliği, e-Fatura/e-Arşiv bağlantısı) burada birikir, `Invoice` tablosu B2B kullanıcılar için anlamsız kolonlarla kirlenmez.

Her iki seçenekte de: müşteri baştan kurumsal fatura isterse `RestaurantCheck.LinkedInvoiceId` doldurulur ve mevcut `InvoicePostingService` akışı olduğu gibi kullanılır — bu kısım zaten nettir, tartışmalı olan yalnızca "fatura istemeyen hızlı satış" durumu.

**Karar (kesinleşti — Karar 1):** Seçenek B, ama önerdiğim `RestaurantRetailSaleDocument` adı yerine ayrı bir **tablo ailesi**: `RetailSale` (başlık) + `RetailSaleLine` (kalemler). Gerekçe (sizin ifadenizle): "Restoranın yoğun perakende fişleri faturaları şişirmesin." `RetailSaleLine`, `RestaurantOrderLine`'a paralel kendi snapshot alanlarını taşır (ürün adı/fiyat/KDV/indirim donmuş), `RestaurantOrderLine`'a FK ile bağlı değildir — kapanışta `RestaurantPostingService` tarafından `RestaurantOrderLine`'lardan üretilir.

`RetailSale` (16. tablo, `RestaurantChecks`'e 1:1 — unique `RestaurantCheckId`):
| Alan | Tip |
|---|---|
| RestaurantCheckId | int (FK, unique) |
| CustomerId | int? (FK→Customer) — opsiyonel |
| DocumentNumber | string(30), unique |
| Status | enum (Issued, Cancelled) |
| SubtotalAmount, DiscountAmount, ServiceChargeAmount, TaxAmount, GrandTotal | decimal(18,2) |
| FiscalDeviceSerialNumber | string(50)? |
| FiscalReceiptNumber | string(50)? |
| ZReportNumber | string(50)? |
| FiscalizationStatus | enum (NotFiscalized, Fiscalized, Failed) — varsayılan NotFiscalized |
| FiscalTransactionId | string(100)? |
| EInvoiceUuid | string(100)? |
| IssuedAtUtc | DateTime |
| CancelledByUserId, CancelledAtUtc, CancellationReason | string?, DateTime?, string? |

`RetailSaleLine` (17. tablo):
| Alan | Tip |
|---|---|
| RetailSaleId | int (FK, Cascade) |
| ProductId | int (FK→Product, Restrict) |
| ProductNameSnapshot | string(200) |
| Quantity | decimal(18,3) |
| UnitPriceSnapshot, TaxRateSnapshot, DiscountAmountSnapshot, LineTotal | decimal |

Ekranda/yazdırılan fişte açıkça **"Dahili Perakende Satış Fişi — Resmî Mali Belge Değildir"** ibaresi bulunur, `FiscalizationStatus = NotFiscalized` olduğu sürece.

---

## 8. Aşamalı Geliştirme ve Test Planı

**Faz 1 — Şema (onaydan sonra):** 15 tablo + `Product.DefaultKitchenStationId`, additive migration, canlıya UYGULANMADAN önce ayrı onay istenir.

**Faz 2 — RestaurantPostingService + servis katmanı:** `OpenTableSessionAsync`, `AddOrderAsync`, `SendToKitchenAsync`, `CloseCheckAsync`, `CancelCheckAsync`, `CancelOrderLineAsync`, `OpenCashShiftAsync`, `CloseCashShiftAsync`. Controller yok, yalnızca servis + birim mantığı, `dotnet build` ile doğrulanır.

**Faz 3 — İlk ekranlar** (kullanıcının belirttiği sırayla): Salon/masa planı → Masa açma+kişi sayısı → Kategori/ürün seçimi (dokunmatik) → Porsiyon/ekstra seçimi → Mutfağa gönder → Mutfak ekranı → Hesap isteme → Parçalı ödeme → Hesap kapatma → Vardiya aç/kapat → Yönetici iptal ekranı.

**Faz 4 — Canlı test (yalnızca E2E-TEST-RESTORAN önekli veriyle, mevcut oturumdaki kurallarla aynı disiplin):**
- Aynı masayı 2 kullanıcının eşzamanlı açması → yalnızca 1 başarılı.
- Adisyonu çift tıklamayla kapatma → tek belge, tek ödeme seti.
- Nakit+kart+yemek kartı parçalı ödeme → toplam tutar eşleşmesi.
- Eksik/fazla ödeme → engellenmeli.
- Mutfak siparişinin mükerrer basılmaması.
- Reçeteye göre doğru hammadde düşümü + fire oranı hesabı.
- Negatif stok kontrolü (mevcut `InventorySettings.AllowNegativeStock` bayrağıyla tutarlı).
- Hesap kapanışında kasıtlı hata enjeksiyonu → TAM rollback (adisyon, ödeme, stok, masa hepsi eski haline döner).
- Ödenmiş adisyon iptali → tek ters kayıt seti, mükerrer değil.
- **Regresyon:** mevcut Fatura/Stok/Cari/Kasa modüllerinde hiçbir bozulma olmamalı (bu oturumdaki gibi, kapsamlı ama yalnızca dokunulan alanlara odaklı).

**Faz 5 (ikinci aşama, bu planın kapsamı dışında):** Masa taşıma, masa birleştirme, adisyon bölme, rezervasyon, paket servis, platform entegrasyonları (Yemeksepeti/Getir/Trendyol tarzı).

---

## 9. Additive Migration Planı — Eski Kayıtları Bozmama Garantisi

- 15 yeni tablo: hiçbiri mevcut tabloya dokunmuyor, tamamen yeni `CREATE TABLE`.
- `Product.DefaultKitchenStationId`: nullable FK, mevcut satırlarda `NULL` — hiçbir eski `Product` kaydı etkilenmez.
- `InventorySettings.IsRestaurantModuleEnabled`: zaten var, varsayılan `false` — migration gerekmez, yalnızca ayarlar ekranından açılır (bu modül bittiğinde).
- Hiçbir mevcut tabloya `NOT NULL` sütun eklenmiyor, hiçbir mevcut check constraint değiştirilmiyor, hiçbir mevcut FK'nin `OnDelete` davranışı değiştirilmiyor.
- Migration'lar `dotnet ef migrations add` ile üretildikten sonra SQL diff'i elle gözden geçirilir (bu oturumda Çek/Senet ve CancelWithPayments değişikliklerinde izlenen disiplinin aynısı).
- Migration'lar yalnızca onayınızdan sonra, önce canlı DB'nin yedeği alınarak uygulanır.

---

## 10. Ekran Rota Haritası (Faz 3 için ön hazırlık, tasarım değil)

| Ekran | Ana aksiyon | RestaurantPostingService çağrısı |
|---|---|---|
| Salon/masa planı | Masaları görüntüle | (salt okuma) |
| Masa açma | Masa aç, kişi sayısı gir | `OpenTableSessionAsync` |
| Ürün seçimi | Sepete ekle | (client-side, henüz kayıt yok) |
| Porsiyon/ekstra | Kalem detaylandır | (client-side) |
| Mutfağa gönder | Siparişi kaydet + fiş oluştur | `AddOrderAsync` → `SendToKitchenAsync` |
| Mutfak ekranı | Durum güncelle | `UpdateKitchenTicketStatusAsync` |
| Hesap isteme | Toplamı göster | (salt okuma, sunucu tekrar hesaplar) |
| Parçalı ödeme | Ödeme satırları gir | (client-side, henüz kayıt yok) |
| Hesap kapatma | Onayla | `CloseCheckAsync` |
| Vardiya aç/kapat | Kasa mutabakatı | `OpenCashShiftAsync` / `CloseCashShiftAsync` |
| Yönetici iptal | Adisyon/satır iptali | `CancelCheckAsync` / `CancelOrderLineAsync` |

---

## 11. Alınan Kararlar (2026-08-03)

Aşağıdaki 5 karar sizin tarafınızdan verildi ve Faz 1 şemasına işlendi:

1. **RetailSale belgesi:** Ayrı bir tablo ailesi — `RetailSale` + `RetailSaleLine`. `Invoice`'a yeni tür EKLENMEDİ. Gerekçe: "Restoranın yoğun perakende fişleri faturaları şişirmesin." (Bkz. §7 — isimlendirme önerdiğim `RestaurantRetailSaleDocument`'tan `RetailSale`/`RetailSaleLine`'a değiştirildi.)
2. **Adisyon/Oturum ilişkisi:** `RestaurantTableSession` **1 → N** `RestaurantCheck`. İlk kullanımda tek adisyon açılır, ama altyapı bölme/taşıma/ayrı hesap senaryolarını baştan destekler (şema zaten bunu doğal olarak sağlıyordu — `RestaurantCheck.RestaurantTableSessionId` bir FK, ek değişiklik gerekmedi).
3. **Roller:** `AppRoles`'e `RestaurantManager`, `Cashier`, `Waiter`, `Kitchen` eklendi. `Administrator` her işleme yetkili olmaya devam ediyor. `IdentitySeed.InitializeAsync` bu 4 rolü otomatik oluşturacak (mevcut `AppRoles.All` döngüsü, ek kod gerekmedi).
4. **KitchenTicketLine durumu:** TERS çevrildi — `KitchenTicketLine` kendi bağımsız `Status` alanını tutar (`RestaurantOrderLine.Status`'tan OKUMAZ), çünkü aynı sipariş kalemi farklı istasyonlara veya tekrar mutfağa gönderilebilir. `RestaurantOrderLine.Status`, Faz 2'de `KitchenTicketLine`'ların durumlarından hesaplanan bir önbellek olacak. Bu nedenle `KitchenTicketLines` üzerindeki eski `(RestaurantOrderLineId)` tekil indexi kaldırıldı, yerine `(KitchenTicketId, RestaurantOrderLineId)` tekil indexi eklendi (bkz. §2 KitchenTicketLines).
5. **Masa birleştirme/taşıma:** Şemaya şimdiden eklendi (öneri: eklenmesindi, ama siz şimdi eklenmesini istediniz) — `RestaurantTableSession.MergedIntoSessionId` (nullable self-FK, birleştirme hedefi) ve yeni `RestaurantTableSessionMove` tablosu (masa taşıma geçmiş kaydı: `RestaurantTableSessionId`, `FromRestaurantTableId`, `ToRestaurantTableId`, `MovedAtUtc`, `MovedByUserId`, `Reason?`). Ekran/akış Faz 5'te gelecek, Faz 1'de yalnızca veri modeli var.

Genel plan onaylandı. Faz 1 (şema + migration) hazırlandı — `AddRestaurantModulePhase1Schema` migration'ı `dotnet build` ile 0 hata doğrulandı, ama canlı veritabanına henüz UYGULANMADI. Migration'ın tam içeriği ve mevcut tablolar üzerindeki etkisi ayrıca (sohbette) raporlandı; production'a uygulama, deploy, commit, push işlemleri sizin ayrı onayınızı bekliyor.
