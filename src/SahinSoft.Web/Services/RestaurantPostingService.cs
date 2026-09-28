using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;
using SahinSoft.Web.Models.Api;

namespace SahinSoft.Web.Services;

// Sepete ürün ekleme/çıkarma, İkram, İndirim ve Not istemci tarafında (JS) tutulur, kayıt oluşmaz —
// tıpkı Teklif Stüdyosu'ndaki satır düzenleme gibi. "Mutfağa Gönder" tıklanınca TÜM parti tek
// atomik çağrıyla RestaurantOrder+RestaurantOrderLines+KitchenTicket(lar)+KitchenTicketLines olarak
// yazılır (bkz. SendOrderToKitchenAsync). Modifier (ekstra/opsiyon) için ayrı bir katalog tablosu
// yok — NameSnapshot/PriceSnapshot çağıran tarafından (gelecekteki ekran) serbest metin olarak
// girilir, doğrulanacak bir DB kaynağı yok.
public sealed record RestaurantOrderLineInput(
    int ProductId,
    int? ProductPortionId,
    decimal Quantity,
    decimal DiscountAmount,
    bool IsComplimentary,
    string? KitchenNote,
    IReadOnlyList<RestaurantOrderLineModifierInput>? Modifiers);

public sealed record RestaurantOrderLineModifierInput(string NameSnapshot, decimal PriceSnapshot, decimal Quantity);

public sealed record RestaurantPaymentInput(RestaurantPaymentMethod Method, int FinancialAccountId, decimal Amount);

// Yazar kasa entegrasyonu açıkken JS tarafı satışı önce fiziksel cihaza gönderir, cihazdan dönen
// fiş/Z no'yu buraya taşır - bkz. SahinSoft.FiscalAgent. null ise (entegrasyon kapalı veya fatura
// kesilen satış) RetailSale bugünkü gibi hiçbir fiskal bilgi olmadan oluşur.
public sealed record FiscalReceiptInfo(string? ReceiptNumber, string? ZNo, string? DeviceSerialNumber);

// UnroutedProductNames: mutfak istasyonu tanımlanmamış ürünler — satır yine de kaydedilir (ör.
// şişe içecek gibi hazırlık gerektirmeyen kalemler için bu normaldir) ama sessizce atlanmaz,
// çağıran (controller) bu listeyi kullanıcıya açıkça göstermek ZORUNDADIR.
public sealed record SendOrderToKitchenResult(RestaurantOrder Order, IReadOnlyList<string> UnroutedProductNames);

public sealed class RestaurantPostingService(
    ApplicationDbContext dbContext,
    DocumentNumberGeneratorService documentNumberGenerator,
    RestaurantPermissionService permissionService,
    InventoryBalanceService inventoryBalance,
    SahinSoft.Web.Services.Printing.PrintDispatchService printDispatchService)
{
    // Masa/Self Satış/Paket - üç "yeni satış başlat" giriş noktasının hepsi bunu çağırır.
    // InventorySettings.RequireOpenShiftForSales kapalıyken (varsayılan) hiçbir şey yapmaz.
    // Açıkken herhangi bir açık RestaurantCashShift var mı diye bakar - hangi kasaya/kullanıcıya
    // ait olduğuna bakmaz (Vardiya'nın FinancialAccountId bazlı kapsam hatası AYRI, henüz
    // düzeltilmedi - burada o hataya bağımlı olmamak için kasıtlı olarak hesap-agnostik).
    private async Task EnsureShiftOpenIfRequiredAsync(CancellationToken cancellationToken)
    {
        var requireOpenShift = await dbContext.InventorySettings
            .Where(x => x.Id == 1)
            .Select(x => x.RequireOpenShiftForSales)
            .SingleOrDefaultAsync(cancellationToken);

        if (!requireOpenShift)
        {
            return;
        }

        var hasOpenShift = await dbContext.RestaurantCashShifts
            .AnyAsync(x => x.Status == RestaurantCashShiftStatus.Open, cancellationToken);

        if (!hasOpenShift)
        {
            throw new InvalidOperationException("Satış başlatmak için önce vardiya açılmalıdır.");
        }
    }

    public Task<(RestaurantTableSession Session, RestaurantCheck Check)> OpenTableSessionAsync(
        int restaurantTableId,
        int guestCount,
        string openedByUserId,
        string? waiterUserId,
        Guid? submissionKey,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                // Çift tıklama/mükerrer POST koruması — aynı SubmissionKey ile daha önce açılmış bir
                // oturum varsa onu (ve adisyonunu) aynen döndür, ikinci bir oturum oluşturma.
                if (submissionKey is not null)
                {
                    var existing = await dbContext.RestaurantTableSessions
                        .Include(x => x.Checks)
                        .SingleOrDefaultAsync(x => x.SubmissionKey == submissionKey, cancellationToken);
                    if (existing is not null)
                    {
                        return (existing, existing.Checks.Single());
                    }
                }

                await EnsureShiftOpenIfRequiredAsync(cancellationToken);

                var table = await dbContext.RestaurantTables
                    .SingleOrDefaultAsync(x => x.Id == restaurantTableId, cancellationToken)
                    ?? throw new InvalidOperationException("Masa bulunamadı.");

                if (!table.IsActive)
                {
                    throw new InvalidOperationException("Bu masa pasif durumda.");
                }

                if (guestCount <= 0)
                {
                    throw new InvalidOperationException("Kişi sayısı sıfırdan büyük olmalıdır.");
                }

                // Aynı masada iki aktif oturum olamaz — DB'deki unique filtered index son güvenlik ağı,
                // burada dostane bir hata olarak erken yakalanır.
                var alreadyOpen = await dbContext.RestaurantTableSessions
                    .AnyAsync(x => x.RestaurantTableId == restaurantTableId && x.Status == RestaurantTableSessionStatus.Open, cancellationToken);
                if (alreadyOpen)
                {
                    throw new InvalidOperationException("Bu masada zaten açık bir oturum var.");
                }

                var session = new RestaurantTableSession
                {
                    RestaurantTableId = restaurantTableId,
                    Status = RestaurantTableSessionStatus.Open,
                    OpenedAtUtc = DateTime.UtcNow,
                    OpenedByUserId = openedByUserId,
                    GuestCount = guestCount,
                    WaiterUserId = waiterUserId,
                    SubmissionKey = submissionKey
                };
                dbContext.RestaurantTableSessions.Add(session);

                var checkNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("RESTAURANT_CHECK", cancellationToken);
                var check = new RestaurantCheck
                {
                    CheckNumber = checkNumber,
                    Status = RestaurantCheckStatus.Open,
                    OpenedAtUtc = DateTime.UtcNow,
                    RestaurantTableSession = session
                };
                dbContext.RestaurantChecks.Add(check);

                // Masa açıldığında üzerindeki rezervasyon (varsa) tüketilmiş sayılır - artık
                // gerçekten oturan bir müşteri var, rozet DOLU'ya döner (Edip, 2026-09-03).
                var activeReservation = await dbContext.RestaurantTableReservations
                    .SingleOrDefaultAsync(x => x.RestaurantTableId == restaurantTableId && x.IsActive, cancellationToken);
                if (activeReservation is not null)
                {
                    activeReservation.IsActive = false;
                    activeReservation.CancelledAtUtc = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return (session, check);
            });
        }, cancellationToken);

    public async Task RequestBillAsync(int checkId, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Bu adisyon artık açık değil.");
        }
        check.BillRequestedAtUtc = DateTime.UtcNow;
        check.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Fiş Notu - kapanmış bir adisyonda da düzenlenebilir (geçmişe dönük "müşteri şikayet etti"
    // gibi bir not eklenmesi gerekebilir), bu yüzden Status kontrolü YOK - RequestBillAsync'in
    // aksine.
    public async Task UpdateCheckNoteAsync(int checkId, string? note, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        check.Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        check.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Cari Ekle (madde 13, onaylı Self Satış tasarımı) - adisyona bir müşteri bağlar/kaldırır.
    // "Açık Hesap" ödeme yöntemi CloseCheckAsync'te bu alanın dolu olmasını ZORUNLU kılar.
    public async Task AttachCustomerAsync(int checkId, int? customerId, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        check.AttachedCustomerId = customerId;
        check.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // "İndirim" (alttaki hızlı işlem / Tahsilat'taki İndirim) - satır bazlı İndirim'den (sabit
    // çubuktaki, tek seçili satıra uygulanan) FARKLI: bu bir TUTAR/YÜZDE indirimini adisyonun
    // TOPLAMINA uygular (Edip, 2026-09-03: "üstte ürünü seçip indirim tuşuna bastığında satır
    // indirimi, altta bastığında indirim tuşuna tutar indirimi toplam tutara, bide tahsilatta
    // indirim o da tutar indirimi sayılsın"). Teknik doküman (2026-09-05, madde 4) BU İKİ İNDİRİM
    // MOTORUNU AYIRDI: adisyon indirimi ARTIK satırlara dağıtılmaz, satırların
    // DiscountAmountSnapshot/UnitPriceSnapshot'ına HİÇ dokunmaz, satırlara "İndirim" etiketi
    // basmaz - sadece RestaurantCheck.TicketDiscountAmount'a yazılır. Toplam hesaplaması
    // (RestaurantController.ComputeCheckRunningTotal, CloseCheckAsync) bu alanı satırların net
    // toplamından ayrıca düşer.
    // GERÇEK HATA (2026-09-05, incelemede bulundu): bu metod HİÇBİR yetki/onay kontrolü
    // yapmıyordu - Ayarlar'daki "İndirim için 2. onay" (RequireSecondApprovalForDiscount) ve
    // CanApplyDiscount izni TANIMLI ama hiçbir yerde KULLANILMIYORDU, yani anlamsızca duruyordu.
    // Artık ToggleLineComplimentaryAsync/ApplyOrderLineDiscountAsync ile AYNI desen: yetkisiz
    // kullanıcı reddedilir, parametre açıksa ikinci yetkili PIN'i şart.
    public async Task ApplyTicketDiscountAsync(
        int checkId,
        decimal totalDiscountAmount,
        string performedByUserId,
        string? approverPin = null,
        CancellationToken cancellationToken = default)
    {
        if (totalDiscountAmount < 0)
        {
            throw new InvalidOperationException("İndirim tutarı negatif olamaz.");
        }

        if (!await permissionService.CanApplyDiscountAsync(performedByUserId, cancellationToken))
        {
            throw new InvalidOperationException("İndirim uygulama yetkiniz yok.");
        }

        string? approverUserId = null;
        if (await permissionService.RequiresSecondApprovalForDiscountAsync(cancellationToken))
        {
            approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanApplyDiscount, cancellationToken)
                ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
        }

        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");

        var netLinesTotal = await dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheckId == checkId && x.Status != RestaurantOrderLineStatus.Cancelled)
            .Select(x => x.Quantity * x.UnitPriceSnapshot - x.DiscountAmountSnapshot)
            .SumAsync(cancellationToken);
        if (netLinesTotal <= 0)
        {
            throw new InvalidOperationException("Adisyonda ürün yok.");
        }

        check.TicketDiscountAmount = Math.Min(totalDiscountAmount, netLinesTotal);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (approverUserId is not null)
        {
            await permissionService.LogApprovalAsync("ApplyTicketDiscount", performedByUserId, approverUserId, checkId, null, $"Tutar: {check.TicketDiscountAmount:N2}", cancellationToken);
        }
    }

    // Fiş İkram (madde 11) - adisyondaki TÜM aktif satırları ikram eder (satır bazlı İkram'ın
    // toplu hali, ApplyTicketDiscountAsync ile AYNI desen). Ciroya dahil DEĞİLDİR - satırların
    // DiscountAmountSnapshot'ı kendi brüt tutarına eşitlenir, tıpkı tekil satır İkram'ı gibi.
    // Yetkili kullanıcı (Administrator) reasonFor/reasonWhy boş bırakabilir, diğerleri (profilinde
    // CanApplyComplimentary açık ama Administrator değil) doldurmak ZORUNDADIR.
    public async Task ApplyReceiptComplimentaryAsync(
        int checkId,
        string performedByUserId,
        string? reasonFor,
        string? reasonWhy,
        string? note,
        CancellationToken cancellationToken = default)
    {
        if (!await permissionService.CanApplyComplimentaryAsync(performedByUserId, cancellationToken))
        {
            throw new InvalidOperationException("İkram uygulama yetkiniz yok.");
        }

        var canSkipReason = await permissionService.IsAdministratorAsync(performedByUserId, cancellationToken);
        if (!canSkipReason && (string.IsNullOrWhiteSpace(reasonFor) || string.IsNullOrWhiteSpace(reasonWhy)))
        {
            throw new InvalidOperationException("Kime ve Neden alanları zorunludur.");
        }

        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Yalnızca açık adisyonlar ikram edilebilir.");
        }

        var lines = await dbContext.RestaurantOrderLines
            .Where(x => x.RestaurantOrder.RestaurantCheckId == checkId && x.Status != RestaurantOrderLineStatus.Cancelled)
            .ToListAsync(cancellationToken);
        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Adisyonda ürün yok.");
        }

        foreach (var line in lines)
        {
            line.IsComplimentary = true;
            line.DiscountAmountSnapshot = line.Quantity * line.UnitPriceSnapshot;
        }

        check.ComplimentaryAtUtc = DateTime.UtcNow;
        check.ComplimentaryByUserId = performedByUserId;
        check.ComplimentaryReasonFor = reasonFor;
        check.ComplimentaryReasonWhy = reasonWhy;
        check.ComplimentaryNote = note;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Fişi Beklet (2026-09-05 teknik doküman madde 8) - check GERÇEKTEN Status=Open kalır (veri
    // bütünlüğü, ödeme geçmişi, kısmi ödemeler bozulmaz), sadece HeldAtUtc/HeldByUserId doldurulur.
    // Boş bir check'i (hiç satırı olmayan) beklemek anlamsız - bu durumda çağıran taraf zaten
    // hiçbir şey göndermemiştir, sessizce no-op geçilir (istisna fırlatılmaz, kasiyer akışı
    // kesilmesin diye).
    public async Task HoldCheckAsync(int checkId, string userId, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks
            .Include(x => x.Orders).ThenInclude(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");

        if (check.Status != RestaurantCheckStatus.Open) return;
        var hasActiveLine = check.Orders.SelectMany(o => o.Lines).Any(l => l.Status != RestaurantOrderLineStatus.Cancelled);
        if (!hasActiveLine) return;

        check.HeldAtUtc = DateTime.UtcNow;
        check.HeldByUserId = userId;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Bekleyen Fişler - "geri çağır" (madde 8) - HeldAtUtc/HeldByUserId temizlenir, check normal
    // açık adisyon gibi Check ekranında görünür. localStorage/URL bayrağı YOK.
    public async Task RecallHeldCheckAsync(int checkId, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        check.HeldAtUtc = null;
        check.HeldByUserId = null;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // "Masayı Boşalt" - müşteri hiçbir şey almadan gitti ya da sipariş edilen her şey iptal
    // edildi; CloseCheckAsync bu durumda "Boş adisyon kapatılamaz" diye reddeder, adisyon açık
    // kalır ve masa DOLU görünmeye devam eder. Bu, ödemesiz bir çıkış yolu: adisyon İPTAL
    // (ödeme/fiş üretimi yok, muhasebeye hiç girmedi zaten) işaretlenir, masa oturumu kapanır,
    // masa yeniden BOŞ'a döner - yeni müşteri tertemiz bir adisyonla başlar (Edip, 2026-09-03:
    // "masa boşaldı tekrar farklı müşteriye gireceğim o iptal hareketleri gelmesin boş gelsin").
    public async Task VoidEmptyCheckAsync(int checkId, string userId, CancellationToken cancellationToken = default)
    {
        var check = await dbContext.RestaurantChecks
            .Include(x => x.Orders).ThenInclude(x => x.Lines)
            .Include(x => x.RestaurantTableSession)
            .SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");

        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Bu adisyon zaten kapalı veya iptal edilmiş.");
        }

        var hasActiveLine = check.Orders.SelectMany(o => o.Lines).Any(l => l.Status != RestaurantOrderLineStatus.Cancelled);
        if (hasActiveLine)
        {
            throw new InvalidOperationException("Adisyonda hâlâ aktif ürün var - önce hepsini iptal edin.");
        }

        var now = DateTime.UtcNow;
        check.Status = RestaurantCheckStatus.Cancelled;
        check.UpdatedAtUtc = now;
        check.RestaurantTableSession.Status = RestaurantTableSessionStatus.Closed;
        check.RestaurantTableSession.ClosedAtUtc = now;
        check.RestaurantTableSession.ClosedByUserId = userId;

        // GERÇEK HATA (2026-09-06, kabul testinde bulundu, madde 20) - kısmi ödemesi olan bir
        // adisyonda "Sipariş Sil" (tüm satırları iptal edip bu metodu tetikleyen) sonrası bu
        // kayıtlar temizlenmiyordu, iptal edilmiş bir adisyona ait öksüz RestaurantCheckPendingPayment
        // satırları kalıyordu. Muhasebeye hiç işlenmedikleri için (bkz. CloseCheckAsync'teki AYNI
        // temizlik) silinmeleri güvenli.
        var pendingPayments = await dbContext.RestaurantCheckPendingPayments
            .Where(x => x.RestaurantCheckId == checkId)
            .ToListAsync(cancellationToken);
        dbContext.RestaurantCheckPendingPayments.RemoveRange(pendingPayments);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Masa başına en fazla bir aktif rezervasyon (DB'deki filtered unique index son güvenlik ağı,
    // bkz. ApplicationDbContext) - boş bir masa rezerve edilebilir, dolu/zaten rezerve bir masa
    // edilemez. Reservation notu/saat/kişi sayısı serbest metin/sayı, doğrulanacak başka bir
    // kaynak yok (MASTER tasarımdaki "21:00 · Doğum günü" gibi).
    public async Task CreateReservationAsync(
        int restaurantTableId,
        DateTime reservedForUtc,
        int guestCount,
        string? note,
        string createdByUserId,
        CancellationToken cancellationToken = default)
    {
        if (guestCount <= 0)
        {
            throw new InvalidOperationException("Kişi sayısı sıfırdan büyük olmalıdır.");
        }

        var table = await dbContext.RestaurantTables.SingleOrDefaultAsync(x => x.Id == restaurantTableId, cancellationToken)
            ?? throw new InvalidOperationException("Masa bulunamadı.");
        if (!table.IsActive)
        {
            throw new InvalidOperationException("Bu masa pasif durumda.");
        }

        var isOccupied = await dbContext.RestaurantTableSessions
            .AnyAsync(x => x.RestaurantTableId == restaurantTableId && x.Status == RestaurantTableSessionStatus.Open, cancellationToken);
        if (isOccupied)
        {
            throw new InvalidOperationException("Bu masa şu an dolu, rezerve edilemez.");
        }

        var alreadyReserved = await dbContext.RestaurantTableReservations
            .AnyAsync(x => x.RestaurantTableId == restaurantTableId && x.IsActive, cancellationToken);
        if (alreadyReserved)
        {
            throw new InvalidOperationException("Bu masa için zaten aktif bir rezervasyon var.");
        }

        dbContext.RestaurantTableReservations.Add(new RestaurantTableReservation
        {
            RestaurantTableId = restaurantTableId,
            ReservedForUtc = reservedForUtc,
            GuestCount = guestCount,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedByUserId = createdByUserId
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelReservationAsync(int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await dbContext.RestaurantTableReservations.SingleOrDefaultAsync(x => x.Id == reservationId, cancellationToken)
            ?? throw new InvalidOperationException("Rezervasyon bulunamadı.");
        reservation.IsActive = false;
        reservation.CancelledAtUtc = DateTime.UtcNow;
        reservation.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Self Satış ve Paket/Gel-Al'ın ORTAK deseni: gerçek bir masaya değil, sistemin gizli
    // (IsActive=false, Masa Satış salon listesinde hiç görünmeyen) bir salonu altında talep
    // üzerine oluşturulan TEK KULLANIMLIK sanal bir masa/oturum/adisyona bağlanırlar - fiyat/
    // ürün/mutfak/ödeme mantığının TAMAMI mevcut RestaurantCheck/RestaurantOrder/KitchenTicket/
    // RestaurantPayment zincirinden değişmeden gelir. ÇAĞIRAN zaten açık bir transaction
    // içinde olmalı - bu yardımcı kendi transaction'ını AÇMAZ.
    private async Task<(RestaurantTableSession Session, RestaurantCheck Check)> CreateHiddenVirtualCheckAsync(
        string hiddenSectionName,
        string tableName,
        int branchId,
        string openedByUserId,
        Guid? submissionKey,
        CancellationToken cancellationToken)
    {
        var section = await dbContext.RestaurantSections
            .SingleOrDefaultAsync(x => x.Name == hiddenSectionName && x.BranchId == branchId, cancellationToken);
        if (section is null)
        {
            section = new RestaurantSection
            {
                Name = hiddenSectionName,
                DisplayOrder = 999,
                IsActive = false, // Masa Satış salon listesinde görünmesin.
                BranchId = branchId
            };
            dbContext.RestaurantSections.Add(section);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var table = new RestaurantTable
        {
            Name = tableName,
            Capacity = 1,
            IsActive = true,
            RestaurantSectionId = section.Id
        };
        dbContext.RestaurantTables.Add(table);
        await dbContext.SaveChangesAsync(cancellationToken);

        var session = new RestaurantTableSession
        {
            RestaurantTableId = table.Id,
            Status = RestaurantTableSessionStatus.Open,
            OpenedAtUtc = DateTime.UtcNow,
            OpenedByUserId = openedByUserId,
            GuestCount = 1,
            SubmissionKey = submissionKey
        };
        dbContext.RestaurantTableSessions.Add(session);

        var checkNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("RESTAURANT_CHECK", cancellationToken);
        var check = new RestaurantCheck
        {
            CheckNumber = checkNumber,
            Status = RestaurantCheckStatus.Open,
            OpenedAtUtc = DateTime.UtcNow,
            RestaurantTableSession = session
        };
        dbContext.RestaurantChecks.Add(check);
        await dbContext.SaveChangesAsync(cancellationToken);

        return (session, check);
    }

    public const string SelfSaleSectionName = "Self Satış";
    private const string PackageSectionName = "Paket";

    // Self Satış varsayılan akışı MASASIZ hızlı satıştır (Edip'in onayı, 2026-08-09: "bir market
    // gibi düşün... normal bir satış masasız"). Kalıcı/paylaşılan TEK bir masa YOKTUR - her satış
    // kendi tek-kullanımlık gizli sanal masasını alır, bu yüzden aynı anda birden çok kasiyer/
    // kiosk çakışmadan çalışabilir. "Benim açık satışım" kavramı kullanıcı bazlı sorgulanır (bkz.
    // RestaurantSelfSaleController) - masa adı sabit "Self Satış" olarak tekrar eder (tekillik
    // şart değil, bkz. RestaurantTable Name index'i unique değil).
    public Task<RestaurantCheck> CreateSelfSaleCheckAsync(
        int branchId,
        string userId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                await EnsureShiftOpenIfRequiredAsync(cancellationToken);

                var (_, check) = await CreateHiddenVirtualCheckAsync(
                    SelfSaleSectionName, SelfSaleSectionName, branchId, userId, submissionKey: null, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return check;
            });
        }, cancellationToken);

    // "Masaya Aktar" - Self Satış'taki açık sepet ödeme alınmadan önce gerçek bir masaya taşınır.
    // Ürün/mutfak fişi İKİ KEZ OLUŞMAZ: RestaurantOrder satırları (ve onlara bağlı KitchenTicket/
    // KitchenTicketLine'lar) SİLİNİP YENİDEN YARATILMAZ, sadece RestaurantCheckId'leri hedef
    // adisyona yeniden bağlanır (Masa Taşı/Birleştir'deki MergeTableSessionsAsync ile birebir aynı
    // desen). Ödeme bu noktada hiç oluşmaz - CloseCheckAsync çağrılmaz. Kaynak Self adisyonu
    // Closed değil Cancelled olarak işaretlenir (tıpkı birleştirmede olduğu gibi) - bu sayede
    // raporlarda tamamlanmış bir Self satışı gibi hiç görünmez (bkz. Karar, Edip 2026-08-09).
    public Task<RestaurantCheck> TransferSelfSaleToTableAsync(
        int selfSaleCheckId,
        int targetTableId,
        string userId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var selfCheck = await dbContext.RestaurantChecks
                    .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
                    .SingleOrDefaultAsync(x => x.Id == selfSaleCheckId, cancellationToken)
                    ?? throw new InvalidOperationException("Adisyon bulunamadı.");

                if (selfCheck.RestaurantTableSession.RestaurantTable.RestaurantSection.Name != SelfSaleSectionName)
                {
                    throw new InvalidOperationException("Yalnızca Self Satış adisyonları bir masaya aktarılabilir.");
                }
                if (selfCheck.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Bu adisyon artık açık değil.");
                }

                var targetTable = await dbContext.RestaurantTables
                    .SingleOrDefaultAsync(x => x.Id == targetTableId, cancellationToken)
                    ?? throw new InvalidOperationException("Hedef masa bulunamadı.");
                if (!targetTable.IsActive)
                {
                    throw new InvalidOperationException("Hedef masa pasif durumda.");
                }

                var targetSession = await dbContext.RestaurantTableSessions
                    .Include(x => x.Checks)
                    .SingleOrDefaultAsync(x => x.RestaurantTableId == targetTableId && x.Status == RestaurantTableSessionStatus.Open, cancellationToken);

                RestaurantCheck targetCheck;
                if (targetSession is not null)
                {
                    // Masada zaten açık adisyon varsa oraya eklenir (mevcut kapanış davranışıyla çelişmez).
                    targetCheck = targetSession.Checks.SingleOrDefault(x => x.Status == RestaurantCheckStatus.Open)
                        ?? throw new InvalidOperationException("Hedef masada açık adisyon bulunamadı.");
                }
                else
                {
                    // Masa boşsa mevcut masa açma kurallarıyla (OpenTableSessionAsync'in ürettiği
                    // oturum/adisyon adımlarıyla birebir aynı) yeni oturum/adisyon açılır.
                    var newSession = new RestaurantTableSession
                    {
                        RestaurantTableId = targetTable.Id,
                        Status = RestaurantTableSessionStatus.Open,
                        OpenedAtUtc = DateTime.UtcNow,
                        OpenedByUserId = userId,
                        GuestCount = 1
                    };
                    dbContext.RestaurantTableSessions.Add(newSession);

                    var newCheckNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("RESTAURANT_CHECK", cancellationToken);
                    var newCheck = new RestaurantCheck
                    {
                        CheckNumber = newCheckNumber,
                        Status = RestaurantCheckStatus.Open,
                        OpenedAtUtc = DateTime.UtcNow,
                        RestaurantTableSession = newSession
                    };
                    dbContext.RestaurantChecks.Add(newCheck);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    targetCheck = newCheck;
                }

                var ordersToMove = await dbContext.RestaurantOrders
                    .Where(x => x.RestaurantCheckId == selfCheck.Id)
                    .ToListAsync(cancellationToken);
                foreach (var order in ordersToMove)
                {
                    order.RestaurantCheckId = targetCheck.Id;
                    order.UpdatedAtUtc = DateTime.UtcNow;
                }

                selfCheck.Status = RestaurantCheckStatus.Cancelled;
                selfCheck.CancelledAtUtc = DateTime.UtcNow;
                selfCheck.CancelledByUserId = userId;
                selfCheck.CancellationReason = $"Masaya aktarıldı (hedef adisyon #{targetCheck.CheckNumber}).";
                selfCheck.RestaurantTableSession.Status = RestaurantTableSessionStatus.Closed;
                selfCheck.RestaurantTableSession.ClosedAtUtc = DateTime.UtcNow;
                selfCheck.RestaurantTableSession.MergedIntoSessionId = targetCheck.RestaurantTableSessionId;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return targetCheck;
            });
        }, cancellationToken);

    public Task<PackageOrder> CreatePackageOrderAsync(
        PackageOrderChannel channel,
        string customerName,
        string? customerPhone,
        string? deliveryAddress,
        int branchId,
        string createdByUserId,
        Guid submissionKey,
        decimal? platformCommissionAmount = null,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var existingByKey = await dbContext.PackageOrders
                    .SingleOrDefaultAsync(x => x.SubmissionKey == submissionKey, cancellationToken);
                if (existingByKey is not null)
                {
                    return existingByKey;
                }

                await EnsureShiftOpenIfRequiredAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(customerName))
                {
                    throw new InvalidOperationException("Müşteri adı zorunludur.");
                }

                // Gel-Al'da adres/telefon şart değil; Telefon/Web siparişinde kurye için ikisi de
                // zorunlu (Edip'in onayı, 2026-08-09).
                if (channel != PackageOrderChannel.PickupInStore)
                {
                    if (string.IsNullOrWhiteSpace(customerPhone))
                    {
                        throw new InvalidOperationException("Telefon/Web siparişlerinde telefon numarası zorunludur.");
                    }
                    if (string.IsNullOrWhiteSpace(deliveryAddress))
                    {
                        throw new InvalidOperationException("Telefon/Web siparişlerinde teslimat adresi zorunludur.");
                    }
                }

                var packageNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("PACKAGE_ORDER", cancellationToken);

                var (_, check) = await CreateHiddenVirtualCheckAsync(
                    PackageSectionName, packageNumber, branchId, createdByUserId, submissionKey, cancellationToken);

                var packageOrder = new PackageOrder
                {
                    PackageNumber = packageNumber,
                    Channel = channel,
                    CustomerName = customerName.Trim(),
                    CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? null : customerPhone.Trim(),
                    DeliveryAddress = string.IsNullOrWhiteSpace(deliveryAddress) ? null : deliveryAddress.Trim(),
                    // Onaylı Paket Operasyon Merkezi mockup'ı (madde birebir-uygulama, 2026-09-05)
                    // "Yeni" durumuyla başlar - kasiyer "Onayla"ya basana kadar mutfağa gitmez
                    // (bkz. AdvancePackageOrderAsync'in New→PendingApproval→Preparing sırası).
                    Status = PackageOrderStatus.New,
                    PlatformCommissionAmount = platformCommissionAmount,
                    SubmissionKey = submissionKey,
                    RestaurantCheck = check
                };
                dbContext.PackageOrders.Add(packageOrder);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return packageOrder;
            });
        }, cancellationToken);

    // Sıradaki durum İSTEMCİDEN parametre olarak asla alınmaz, her zaman sunucuda MEVCUT
    // durumdan hesaplanır - "Hazırlanıyor" iken "Yolda"ya sıçrama yapısal olarak imkansızdır.
    // Gel-Al siparişlerinde kurye adımları (Kurye Bekliyor/Yolda) atlanır. Teslim Edildi
    // terminaldir. SubmissionKey ile aynı butona çift tıklama/mükerrer POST'ta ikinci çağrı
    // no-op olarak mevcut kaydı döndürür (RowVersion optimistic concurrency + retry katmanı
    // gerçek eşzamanlı çift POST'u da güvenli hale getirir).
    public Task<PackageOrder> AdvancePackageOrderAsync(
        int packageOrderId,
        Guid submissionKey,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var packageOrder = await dbContext.PackageOrders
                    .SingleOrDefaultAsync(x => x.Id == packageOrderId, cancellationToken)
                    ?? throw new InvalidOperationException("Paket siparişi bulunamadı.");

                if (packageOrder.SubmissionKey == submissionKey)
                {
                    return packageOrder;
                }

                var nextStatus = (packageOrder.Status, packageOrder.Channel) switch
                {
                    // Onaylı mockup'ın "Yeni"/"Onay Bekliyor" aşamaları (madde birebir-uygulama,
                    // 2026-09-05) - kasiyer siparişi görüp kabul ettiğini, SONRA mutfağa
                    // gönderilmeye hazır olduğunu iki AYRI adımda onaylar.
                    (PackageOrderStatus.New, _) => PackageOrderStatus.PendingApproval,
                    (PackageOrderStatus.PendingApproval, _) => PackageOrderStatus.Preparing,
                    (PackageOrderStatus.Preparing, _) => PackageOrderStatus.Ready,
                    (PackageOrderStatus.Ready, PackageOrderChannel.PickupInStore) => PackageOrderStatus.Delivered,
                    (PackageOrderStatus.Ready, _) => PackageOrderStatus.CourierWaiting,
                    (PackageOrderStatus.CourierWaiting, _) => PackageOrderStatus.OnTheWay,
                    (PackageOrderStatus.OnTheWay, _) => PackageOrderStatus.Delivered,
                    (PackageOrderStatus.Delivered, _) => throw new InvalidOperationException("Bu sipariş zaten teslim edilmiş."),
                    (PackageOrderStatus.Cancelled, _) => throw new InvalidOperationException("Bu sipariş iptal edilmiş."),
                    _ => throw new InvalidOperationException("Beklenmeyen sipariş durumu.")
                };

                packageOrder.Status = nextStatus;
                packageOrder.SubmissionKey = submissionKey;
                var now = DateTime.UtcNow;
                switch (nextStatus)
                {
                    case PackageOrderStatus.Ready:
                        packageOrder.ReadyAtUtc = now;
                        break;
                    case PackageOrderStatus.OnTheWay:
                        packageOrder.DispatchedAtUtc = now;
                        break;
                    case PackageOrderStatus.Delivered:
                        packageOrder.DeliveredAtUtc = now;
                        break;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return packageOrder;
            });
        }, cancellationToken);

    // "İptal Et" (madde birebir-uygulama, Paket Operasyon Merkezi, 2026-09-05) - fiş/satır
    // seviyesinde DEĞİL, siparişin TAMAMI seviyesinde bir iptal. Fiş-iptal yetkisiyle (madde 21,
    // CanCancelReceipt) AYNI yetki kapısını kullanır - yeni bir izin bayrağı İCAT EDİLMEDİ, bu
    // zaten "bir siparişi/fişi tamamen iptal etme" işleminin GERÇEK karşılığı. Aktif satırları
    // toplu iptal edip ardından VoidEmptyCheckAsync ile AYNI mantıkla adisyonu kapatır.
    public async Task CancelPackageOrderAsync(int packageOrderId, string cancelledByUserId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }
        if (!await permissionService.CanCancelReceiptAsync(cancelledByUserId, cancellationToken))
        {
            throw new InvalidOperationException("Sipariş iptal etme yetkiniz yok.");
        }

        await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync<object?>(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var packageOrder = await dbContext.PackageOrders
                    .Include(x => x.RestaurantCheck).ThenInclude(x => x.Orders).ThenInclude(x => x.Lines)
                    .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession)
                    .SingleOrDefaultAsync(x => x.Id == packageOrderId, cancellationToken)
                    ?? throw new InvalidOperationException("Paket siparişi bulunamadı.");

                if (packageOrder.Status is PackageOrderStatus.Delivered or PackageOrderStatus.Cancelled)
                {
                    throw new InvalidOperationException("Bu sipariş zaten teslim edilmiş veya iptal edilmiş.");
                }

                var now = DateTime.UtcNow;
                foreach (var line in packageOrder.RestaurantCheck.Orders.SelectMany(o => o.Lines).Where(l => l.Status != RestaurantOrderLineStatus.Cancelled))
                {
                    line.Status = RestaurantOrderLineStatus.Cancelled;
                    line.UpdatedAtUtc = now;
                }

                packageOrder.Status = PackageOrderStatus.Cancelled;
                packageOrder.CancellationReason = reason.Trim();
                packageOrder.UpdatedAtUtc = now;

                var check = packageOrder.RestaurantCheck;
                check.Status = RestaurantCheckStatus.Cancelled;
                check.UpdatedAtUtc = now;
                check.RestaurantTableSession.Status = RestaurantTableSessionStatus.Closed;
                check.RestaurantTableSession.ClosedAtUtc = now;
                check.RestaurantTableSession.ClosedByUserId = cancelledByUserId;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return (object?)null;
            });
        }, cancellationToken);
    }

    // Kurye Ata (madde birebir-uygulama, Paket Operasyon Merkezi, 2026-09-05) - basit atama,
    // gerçek bir GPS/rota hesabı YOK. Kurye "Teslimatta" durumuna otomatik geçer (kasiyer elle
    // "Müsait"e geri almalı - ayrı bir "teslim tamamlandı → otomatik müsait" akışı İCAT EDİLMEDİ,
    // spec'in istediği kadarıyla sınırlı).
    public async Task AssignCourierAsync(int packageOrderId, int courierId, CancellationToken cancellationToken = default)
    {
        var packageOrder = await dbContext.PackageOrders.SingleOrDefaultAsync(x => x.Id == packageOrderId, cancellationToken)
            ?? throw new InvalidOperationException("Paket siparişi bulunamadı.");
        var courier = await dbContext.RestaurantCouriers.SingleOrDefaultAsync(x => x.Id == courierId && x.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Kurye bulunamadı.");

        packageOrder.AssignedCourierId = courier.Id;
        packageOrder.UpdatedAtUtc = DateTime.UtcNow;
        courier.Status = CourierStatus.Delivering;
        courier.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<SendOrderToKitchenResult> SendOrderToKitchenAsync(
        int restaurantCheckId,
        IReadOnlyList<RestaurantOrderLineInput> lines,
        string orderedByUserId,
        Guid? submissionKey,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(() => SendOrderToKitchenCoreAsync(restaurantCheckId, lines, orderedByUserId, submissionKey, cancellationToken));
        }, cancellationToken);

    private async Task<SendOrderToKitchenResult> SendOrderToKitchenCoreAsync(
        int restaurantCheckId,
        IReadOnlyList<RestaurantOrderLineInput> lines,
        string orderedByUserId,
        Guid? submissionKey,
        CancellationToken cancellationToken)
    {
        dbContext.ChangeTracker.Clear();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        // Aynı siparişin iki kez mutfağa gönderilmesini engeller — çift tıklama/ağ tekrarında aynı
        // SubmissionKey ile gelen istek, yeni satır/fiş oluşturmadan mevcut siparişi döndürür.
        if (submissionKey is not null)
        {
            var existingOrder = await dbContext.RestaurantOrders
                .Include(x => x.Lines)
                .SingleOrDefaultAsync(x => x.SubmissionKey == submissionKey, cancellationToken);
            if (existingOrder is not null)
            {
                var unroutedAgain = existingOrder.Lines
                    .Where(x => x.Status != RestaurantOrderLineStatus.Cancelled)
                    .Where(x => !dbContext.KitchenTicketLines.Any(t => t.RestaurantOrderLineId == x.Id))
                    .Select(x => x.ProductNameSnapshot)
                    .Distinct()
                    .ToList();
                return new SendOrderToKitchenResult(existingOrder, unroutedAgain);
            }
        }

        if (lines.Count == 0)
        {
            throw new InvalidOperationException("Gönderilecek en az bir sipariş satırı olmalıdır.");
        }

        var check = await dbContext.RestaurantChecks
            .Include(x => x.RestaurantTableSession).ThenInclude(x => x.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .SingleOrDefaultAsync(x => x.Id == restaurantCheckId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");

        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Yalnızca açık adisyonlara sipariş eklenebilir.");
        }

        // GERÇEK HATA (2026-09-25, Edip: "self satış mutfakta bir işi yok hangi bölümde olursa
        // olsun mutfağa gönder demediği sürece") - Self Satış'ın etiketli "Mutfağa Gönder" butonu
        // ve yan "Mutfak" ikonu UI'da zaten gizli, ama bu metot flushCartToKitchen üzerinden HIZLI
        // ÖDEME/ADİSYON butonlarından da (satır/sipariş kaydı için) çağrılıyor - kanal farkı
        // gözetmeden KDS açıkken istasyonu olan her ürün gerçek bir KitchenTicket'a giriyordu.
        // Sunucu tarafında KESİN bir kural olarak sabitlendi: Self Satış kanalı asla gerçek mutfak
        // fişi üretmez - sipariş satırları yine de normal şekilde kaydedilir (stok/muhasebe için
        // gerekli), yalnızca aşağıdaki istasyon yönlendirme/KitchenTicket bloğu bu kanal için
        // atlanır.
        var isSelfSaleChannel = check.RestaurantTableSession.RestaurantTable.RestaurantSection.Name == SelfSaleSectionName;

        var order = new RestaurantOrder
        {
            RestaurantCheckId = check.Id,
            OrderedAtUtc = DateTime.UtcNow,
            OrderedByUserId = orderedByUserId,
            SubmissionKey = submissionKey
        };
        dbContext.RestaurantOrders.Add(order);

        var orderLines = new List<RestaurantOrderLine>();
        foreach (var input in lines)
        {
            if (input.Quantity <= 0)
            {
                throw new InvalidOperationException("Sipariş miktarı sıfırdan büyük olmalıdır.");
            }

            // Fiyat/porsiyon/ürün adı istemciden GELEN değere güvenilmeden, DB'den taze okunarak
            // doğrulanır ve donan snapshot bu sunucu değerlerinden üretilir — tarayıcı yalnızca
            // hangi ürün/porsiyon/miktarın seçildiğini belirtir.
            var product = await dbContext.Products
                .Include(x => x.TaxRate)
                .SingleOrDefaultAsync(x => x.Id == input.ProductId && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException($"Ürün bulunamadı veya pasif (Id={input.ProductId}).");

            ProductPortion? portion = null;
            if (input.ProductPortionId is int portionId)
            {
                portion = await dbContext.ProductPortions
                    .SingleOrDefaultAsync(x => x.Id == portionId && x.ProductId == input.ProductId && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException($"{product.Name} için geçersiz porsiyon seçimi.");
            }

            var unitPrice = portion?.PriceOverride ?? product.SalePrice;
            var gross = Math.Round(input.Quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

            // İndirim tutarı çağırandan gelir (personel elle indirim uygular) ama [0, brüt tutar]
            // aralığına sıkıştırılır — sunucu tarafında hesaplanan brüt tutarı asla aşamaz veya
            // negatif olamaz. İkram işaretliyse indirim tutarı brüt tutara eşitlenir (tam ücretsiz).
            var discount = input.IsComplimentary
                ? gross
                : Math.Clamp(input.DiscountAmount, 0, gross);

            var line = new RestaurantOrderLine
            {
                RestaurantOrder = order,
                ProductId = product.Id,
                ProductPortionId = portion?.Id,
                Quantity = input.Quantity,
                ProductNameSnapshot = product.Name,
                PortionNameSnapshot = portion?.Name,
                UnitPriceSnapshot = unitPrice,
                TaxRateSnapshot = product.TaxRate.Rate,
                DiscountAmountSnapshot = discount,
                IsComplimentary = input.IsComplimentary,
                KitchenNote = string.IsNullOrWhiteSpace(input.KitchenNote) ? null : input.KitchenNote.Trim(),
                Status = RestaurantOrderLineStatus.Ordered
            };
            dbContext.RestaurantOrderLines.Add(line);
            orderLines.Add(line);

            if (input.Modifiers is not null)
            {
                foreach (var modifierInput in input.Modifiers)
                {
                    if (modifierInput.Quantity <= 0)
                    {
                        throw new InvalidOperationException("Ekstra/opsiyon miktarı sıfırdan büyük olmalıdır.");
                    }

                    dbContext.RestaurantOrderLineModifiers.Add(new RestaurantOrderLineModifier
                    {
                        RestaurantOrderLine = line,
                        NameSnapshot = modifierInput.NameSnapshot.Trim(),
                        PriceSnapshot = Math.Max(0, modifierInput.PriceSnapshot),
                        Quantity = modifierInput.Quantity
                    });
                }
            }
        }

        if (isSelfSaleChannel)
        {
            foreach (var line in orderLines)
            {
                line.Status = RestaurantOrderLineStatus.Served;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SendOrderToKitchenResult(order, []);
        }

        // GERÇEK HATA (2026-09-28, Edip: "sistem acik olsun olmasin mutfaga cikti gonderebilsin
        // KDS sistemi farkli bir olay") - "Mutfak KDS Takibi" ayarı SADECE Mutfak ekranındaki
        // Sent/InProgress/Ready KUYRUĞUNUN görünürlüğünü kontrol eder (personelin Hazır/Servis
        // Edildi tıklaması gereken aktif bir ekran akışı mı, yoksa hiç). Fiziksel yazıcıya basma
        // bu ayardan TAMAMEN bağımsızdır - bu yüzden istasyon yönlendirme + KitchenTicket
        // oluşturma HER ZAMAN çalışır (aşağıdaki yazdırma döngüsü ticket.Id'ye ihtiyaç duyuyor).
        // Yalnızca BAŞLANGIÇ durumu farklı: takip açıkken Sent/Preparing (KDS ekranında görünür,
        // personel ilerletir), kapalıyken doğrudan Served (KDS ekranına hiç girmez, dashboard/
        // nav'daki "bekleyen" sayaçlarını hiç etkilemez) - ama HER İKİ durumda da fiş basılır.
        var isKitchenTrackingEnabled = await dbContext.InventorySettings
            .Where(x => x.Id == 1)
            .Select(x => x.IsKitchenTrackingEnabled)
            .SingleOrDefaultAsync(cancellationToken);

        var initialTicketStatus = isKitchenTrackingEnabled ? KitchenTicketStatus.Sent : KitchenTicketStatus.Served;
        var initialTicketLineStatus = isKitchenTrackingEnabled ? KitchenTicketLineStatus.Sent : KitchenTicketLineStatus.Served;
        var initialOrderLineStatus = isKitchenTrackingEnabled ? RestaurantOrderLineStatus.Preparing : RestaurantOrderLineStatus.Served;

        // Ürünün varsayılan mutfak istasyonuna göre grupla. İstasyonu olmayan ürünler mutfak fişine
        // eklenmez ama kalemin kendisi kaydedilir (ör. şişe içecek) — yalnızca sessizce atlanmaz,
        // isim listesi çağırana döndürülür (bkz. SendOrderToKitchenResult.UnroutedProductNames).
        var productIds = orderLines.Select(x => x.ProductId).Distinct().ToList();
        var stationByProduct = await dbContext.Products
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DefaultKitchenStationId })
            .ToDictionaryAsync(x => x.Id, x => x.DefaultKitchenStationId, cancellationToken);

        var unroutedProductNames = new List<string>();
        var linesByStation = new Dictionary<int, List<RestaurantOrderLine>>();
        foreach (var line in orderLines)
        {
            var stationId = stationByProduct.GetValueOrDefault(line.ProductId);
            if (stationId is null)
            {
                unroutedProductNames.Add(line.ProductNameSnapshot);
                // İstasyonu yok - ne KDS ekranına ne yazıcıya gider. Takip açıkken eski davranış
                // korunur (dokunulmaz, Ordered'da kalır); kapalıyken KDS hiç yokmuş gibi satır
                // doğrudan Servis Edildi sayılır.
                if (!isKitchenTrackingEnabled)
                {
                    line.Status = RestaurantOrderLineStatus.Served;
                }
                continue;
            }

            if (!linesByStation.TryGetValue(stationId.Value, out var group))
            {
                group = [];
                linesByStation[stationId.Value] = group;
            }
            group.Add(line);
        }

        var createdTickets = new List<(KitchenTicket Ticket, int StationId)>();
        foreach (var (stationId, groupLines) in linesByStation)
        {
            var station = await dbContext.KitchenStations
                .SingleAsync(x => x.Id == stationId, cancellationToken);

            var ticket = new KitchenTicket
            {
                RestaurantOrder = order,
                KitchenStationId = stationId,
                Status = initialTicketStatus,
                SentAtUtc = DateTime.UtcNow
            };
            dbContext.KitchenTickets.Add(ticket);

            foreach (var line in groupLines)
            {
                dbContext.KitchenTicketLines.Add(new KitchenTicketLine
                {
                    KitchenTicket = ticket,
                    RestaurantOrderLine = line,
                    Status = initialTicketLineStatus
                });
                // Fiş gönderildiği an satırın hazırlık durumu Preparing'e geçer (takip açıkken) —
                // kalan durum geçişleri (InProgress/Ready/Served) Faz 3'teki mutfak ekranından
                // KitchenTicketLine üzerinden yapılır. Takip kapalıyken satır doğrudan Served olur.
                line.Status = initialOrderLineStatus;
            }

            // Fiziksel yazıcıya HENÜZ hiçbir şey gönderilmez — bu yalnızca kuyruk kaydıdır. Gelecekteki
            // bir yazıcı ajanı bu tabloyu (ProcessedAtUtc IS NULL) poll ederek gerçek çıktıyı üretecek
            // şekilde tasarlandı; EventType/PayloadJson o ajanın ihtiyaç duyacağı asgari bilgiyi taşır.
            dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
            {
                EventType = "KitchenTicketCreated",
                PayloadJson = JsonSerializer.Serialize(new
                {
                    ticket.RecordId,
                    StationId = stationId,
                    StationName = station.Name,
                    station.PrinterName,
                    Lines = groupLines.Select(x => new { x.ProductNameSnapshot, x.PortionNameSnapshot, x.Quantity, x.KitchenNote }).ToList()
                })
            });

            createdTickets.Add((ticket, stationId));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Çıktı Tasarımcısı entegrasyonu (kademeli geçiş, adım 4b: Mutfak Fişi) - "Mutfak KDS
        // Takibi" ayarı kapalı olsa bile HER ZAMAN çalışır (yukarıdaki initialTicketStatus notuna
        // bkz.) - KDS ekran takibi ile fiziksel yazdırma birbirinden bağımsız. Transaction
        // KAPANDIKTAN SONRA, best-effort - yazıcı tanımlı değilse sessizce atlanır.
        foreach (var (ticket, stationId) in createdTickets)
        {
            try
            {
                await printDispatchService.EnqueueForRoleAsync(
                    Domain.Enums.PrinterRole.Mutfak,
                    check.RestaurantTableSession.RestaurantTable.RestaurantSection.BranchId,
                    Domain.Enums.PrintTemplateType.MutfakFisi,
                    ticket.Id,
                    $"Mutfak fişi - {ticket.TicketNumber ?? ("#" + ticket.Id)}",
                    cancellationToken,
                    kitchenStationId: stationId);
            }
            catch
            {
                // Sipariş zaten mutfağa/KDS'ye ulaştı - yazdırma kuyruğa alınamasa bile burada
                // fırlatmak siparişi geri almaz.
            }
        }

        return new SendOrderToKitchenResult(order, unroutedProductNames);
    }

    public async Task CancelOrderLineAsync(
        int restaurantOrderLineId,
        string cancelledByUserId,
        string reason,
        string? approverPin = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        var affectedCheckId = await DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                // Yetki Mimarisi (Edip, 2026-09-04, madde 9/21/25) - eski "satır kilidi" mantığı
                // TAMAMEN KALDIRILDI, yerine yetki kontrolü geçti. Mutfağa gönderilmiş/gönderilmemiş
                // ayrımı YOK - kullanıcının profilinde CanCancelOrderLine kapalıysa iptal edemez,
                // açıksa (veya hiç profili yoksa) her koşulda edebilir.
                if (!await permissionService.CanCancelOrderLineAsync(cancelledByUserId, cancellationToken))
                {
                    throw new InvalidOperationException("Sipariş satırı iptal etme yetkiniz yok.");
                }

                // "Şifre sorulsun mu?" (madde 21) - kullanıcı yetkili olsa BİLE parametre açıksa
                // ikinci bir yetkilinin PIN onayı şart, onay MUTLAKA loglanır.
                string? approverUserId = null;
                if (await permissionService.RequiresSecondApprovalForCancelOrderLineAsync(cancellationToken))
                {
                    approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanCancelOrderLine, cancellationToken)
                        ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var line = await dbContext.RestaurantOrderLines
                    .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck)
                    .Include(x => x.KitchenTicketLines)
                    .SingleOrDefaultAsync(x => x.Id == restaurantOrderLineId, cancellationToken)
                    ?? throw new InvalidOperationException("Sipariş satırı bulunamadı.");

                if (line.RestaurantOrder.RestaurantCheck.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık adisyondaki satırlar buradan iptal edilebilir.");
                }

                if (line.Status == RestaurantOrderLineStatus.Cancelled)
                {
                    throw new InvalidOperationException("Bu satır zaten iptal edilmiş.");
                }

                // Hard-delete YOK — orijinal satır ve mutfak fiş satırı korunur, yalnızca durumları
                // Cancelled'a çevrilir. Mutfağa gönderilmiş bir satırsa (KitchenTicketLines doluysa)
                // her fiş satırı için ayrı bir "iptal bildirimi" outbox kaydı oluşturulur ki mutfak
                // tarafı (ekran veya ileride yazıcı ajanı) bunu görebilsin.
                line.Status = RestaurantOrderLineStatus.Cancelled;
                line.CancelledByUserId = cancelledByUserId;
                line.CancelledAtUtc = DateTime.UtcNow;
                line.CancellationReason = reason;

                foreach (var ticketLine in line.KitchenTicketLines.Where(x => x.Status != KitchenTicketLineStatus.Cancelled))
                {
                    ticketLine.Status = KitchenTicketLineStatus.Cancelled;
                    dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
                    {
                        EventType = "KitchenTicketLineCancelled",
                        PayloadJson = JsonSerializer.Serialize(new
                        {
                            KitchenTicketLineRecordId = ticketLine.RecordId,
                            KitchenTicketId = ticketLine.KitchenTicketId,
                            line.ProductNameSnapshot,
                            line.PortionNameSnapshot,
                            Reason = reason
                        })
                    });
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (approverUserId is not null)
                {
                    await permissionService.LogApprovalAsync("CancelOrderLine", cancelledByUserId, approverUserId, line.RestaurantOrder.RestaurantCheckId, line.Id, reason, cancellationToken);
                }

                return line.RestaurantOrder.RestaurantCheckId;
            });
        }, cancellationToken);

        // Boş Adisyon (madde 20, Edip 2026-09-04: "Self Satış'ta son ürün de iptal edilince
        // adisyon KENDİLİĞİNDEN temizlenir") - önceden SADECE Self Satış'a sınırlıydı ("Masa
        // Satış'ta müşteri hâlâ masada oturuyor olabilir" endişesiyle). 2026-09-05 teknik
        // doküman (madde 13) bunu AÇIKÇA Masa Satış'a da genişletti: "Tüm adisyon silinirse
        // masa 0,00 açık kalmayacak, Boş durumuna dönecek." - bu yeni karar eskisinin YERİNE
        // geçer, artık check'in hangi bölümde olduğuna bakılmaksızın (Self/Masa/Paket) çalışır.
        try
        {
            await VoidEmptyCheckAsync(affectedCheckId, cancelledByUserId, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Adisyonda hâlâ aktif ürün var (bu iptal SONUNCUSU değildi) - normal durum,
            // sessizce yok say.
        }
    }

    // "Sipariş Sil" (Edip, 2026-09-05: "yetkisi varsa her koşulda çalışsın, ödeme ekranından
    // Adisyona Dön sonrası aktif olmasın diye bir sebep yok") - önceden SADECE istemcideki henüz
    // gönderilmemiş sepeti (JS cart dizisi) temizliyordu; ödeme ekranına girip Adisyona Dön
    // sonrası TÜM satırlar zaten sunucuya gönderilmiş (cart boş) olduğundan buton görünüşte
    // hiçbir şey yapmıyordu. Artık adisyondaki GÖNDERİLMİŞ satırları da (ayrı ayrı CancelOrderLine
    // çağırmadan, TEK yetki/onay ile) topluca iptal eder - satır bazlı iptalle AYNI reversal
    // mantığı (KitchenTicketLine iptali/outbox), sadece TEK gerekçe/PIN ile TÜM satırlara uygulanır.
    // Son satır da iptal olduğundan adisyon kendiliğinden boşalır (VoidEmptyCheckAsync).
    public Task ClearOrderAsync(
        int checkId,
        string performedByUserId,
        string reason,
        string? approverPin = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                if (!await permissionService.CanClearOrderAsync(performedByUserId, cancellationToken))
                {
                    throw new InvalidOperationException("Siparişi silme yetkiniz yok.");
                }

                string? approverUserId = null;
                if (await permissionService.RequiresSecondApprovalForCancelOrderLineAsync(cancellationToken))
                {
                    approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanCancelOrderLine, cancellationToken)
                        ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var check = await dbContext.RestaurantChecks
                    .Include(x => x.Orders).ThenInclude(x => x.Lines).ThenInclude(x => x.KitchenTicketLines)
                    .Include(x => x.RestaurantTableSession)
                    .SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
                    ?? throw new InvalidOperationException("Adisyon bulunamadı.");

                if (check.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık adisyonlar silinebilir.");
                }

                var activeLines = check.Orders.SelectMany(o => o.Lines).Where(l => l.Status != RestaurantOrderLineStatus.Cancelled).ToList();
                if (activeLines.Count == 0)
                {
                    throw new InvalidOperationException("Adisyonda silinecek ürün yok.");
                }

                var now = DateTime.UtcNow;
                foreach (var line in activeLines)
                {
                    line.Status = RestaurantOrderLineStatus.Cancelled;
                    line.CancelledByUserId = performedByUserId;
                    line.CancelledAtUtc = now;
                    line.CancellationReason = reason;

                    foreach (var ticketLine in line.KitchenTicketLines.Where(x => x.Status != KitchenTicketLineStatus.Cancelled))
                    {
                        ticketLine.Status = KitchenTicketLineStatus.Cancelled;
                        dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
                        {
                            EventType = "KitchenTicketLineCancelled",
                            PayloadJson = JsonSerializer.Serialize(new
                            {
                                KitchenTicketLineRecordId = ticketLine.RecordId,
                                KitchenTicketId = ticketLine.KitchenTicketId,
                                line.ProductNameSnapshot,
                                line.PortionNameSnapshot,
                                Reason = reason
                            })
                        });
                    }
                }

                // Tüm satırlar iptal edildi - adisyon kendiliğinden boşalır (madde 13/20 ile AYNI
                // davranış, VoidEmptyCheckAsync'i burada TEKRAR YAZMADAN inline uyguluyoruz çünkü
                // check zaten bu transaction içinde Include'lu yüklü).
                check.Status = RestaurantCheckStatus.Cancelled;
                check.UpdatedAtUtc = now;
                check.RestaurantTableSession.Status = RestaurantTableSessionStatus.Closed;
                check.RestaurantTableSession.ClosedAtUtc = now;
                check.RestaurantTableSession.ClosedByUserId = performedByUserId;

                // GERÇEK HATA (2026-09-06, kabul testinde bulundu, madde 20) - VoidEmptyCheckAsync'e
                // eklenen AYNI temizlik burada da gerekli: kısmi ödemesi olan bir adisyonda
                // "Sipariş Sil" öksüz RestaurantCheckPendingPayment satırları bırakıyordu.
                var pendingPayments = await dbContext.RestaurantCheckPendingPayments
                    .Where(x => x.RestaurantCheckId == checkId)
                    .ToListAsync(cancellationToken);
                dbContext.RestaurantCheckPendingPayments.RemoveRange(pendingPayments);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (approverUserId is not null)
                {
                    await permissionService.LogApprovalAsync("ClearOrder", performedByUserId, approverUserId, checkId, null, reason, cancellationToken);
                }

                return true;
            });
        }, cancellationToken);
    }

    // Mutfağa gönderilmiş bir satırın miktarını sonradan düzeltme - Edip, 2026-09-03: "mutfağa
    // gönderildi diye herşeyi pasif hale getirme, miktar düzeltme aktif olsun". KitchenTicketLine
    // kendi miktarını tutmaz (RestaurantOrderLine.Quantity'den okunur), bu yüzden tek satır
    // güncellemesi yeterli.
    public Task AdjustOrderLineQuantityAsync(
        int restaurantOrderLineId,
        decimal newQuantity,
        string performedByUserId,
        string? approverPin = null,
        CancellationToken cancellationToken = default)
    {
        if (newQuantity <= 0)
        {
            throw new InvalidOperationException("Miktar sıfırdan büyük olmalıdır.");
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                // Madde 9/21/25 - satır kilidi kaldırıldı, yetki kontrolü geçti (bkz.
                // CancelOrderLineAsync'teki AYNI not).
                if (!await permissionService.CanEditKitchenSentLinesAsync(performedByUserId, cancellationToken))
                {
                    throw new InvalidOperationException("Mutfağa gönderilmiş ürünü düzenleme yetkiniz yok.");
                }

                string? approverUserId = null;
                if (await permissionService.RequiresSecondApprovalForEditKitchenSentLinesAsync(cancellationToken))
                {
                    approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanEditKitchenSentLines, cancellationToken)
                        ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var line = await dbContext.RestaurantOrderLines
                    .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck)
                    .SingleOrDefaultAsync(x => x.Id == restaurantOrderLineId, cancellationToken)
                    ?? throw new InvalidOperationException("Sipariş satırı bulunamadı.");

                if (line.RestaurantOrder.RestaurantCheck.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık adisyondaki satırlar düzenlenebilir.");
                }

                if (line.Status == RestaurantOrderLineStatus.Cancelled)
                {
                    throw new InvalidOperationException("İptal edilmiş satır düzenlenemez.");
                }

                line.Quantity = newQuantity;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (approverUserId is not null)
                {
                    await permissionService.LogApprovalAsync("AdjustOrderLineQuantity", performedByUserId, approverUserId, line.RestaurantOrder.RestaurantCheckId, line.Id, $"Yeni miktar: {newQuantity}", cancellationToken);
                }

                return true;
            });
        }, cancellationToken);
    }

    // Mutfağa gönderilmiş TEK bir satıra indirim - Edip, 2026-09-05: "bu satır indirimini yapsın
    // bu hatayı vermesin üst butonlar aktif çalışsın" (önceki hali sadece bir uyarı gösterip
    // kullanıcıyı fiş geneli %İndirim'e yönlendiriyordu, gerçek bir uç nokta yoktu). Yetki/onay
    // deseni ApplyTicketDiscountAsync ile AYNI (CanApplyDiscount/RequireSecondApprovalForDiscount) -
    // bu bir "satır düzenleme" değil, bir İNDİRİM işlemi; incelemede CanEditKitchenSentLines'a
    // yanlışlıkla bağlanmış olduğu bulunup düzeltildi (2026-09-05).
    public Task ApplyOrderLineDiscountAsync(
        int restaurantOrderLineId,
        decimal discountAmount,
        string performedByUserId,
        string? approverPin = null,
        CancellationToken cancellationToken = default)
    {
        if (discountAmount < 0)
        {
            throw new InvalidOperationException("İndirim tutarı negatif olamaz.");
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                if (!await permissionService.CanApplyDiscountAsync(performedByUserId, cancellationToken))
                {
                    throw new InvalidOperationException("İndirim uygulama yetkiniz yok.");
                }

                string? approverUserId = null;
                if (await permissionService.RequiresSecondApprovalForDiscountAsync(cancellationToken))
                {
                    approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanApplyDiscount, cancellationToken)
                        ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var line = await dbContext.RestaurantOrderLines
                    .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck)
                    .SingleOrDefaultAsync(x => x.Id == restaurantOrderLineId, cancellationToken)
                    ?? throw new InvalidOperationException("Sipariş satırı bulunamadı.");

                if (line.RestaurantOrder.RestaurantCheck.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık adisyondaki satırlar düzenlenebilir.");
                }

                if (line.Status == RestaurantOrderLineStatus.Cancelled)
                {
                    throw new InvalidOperationException("İptal edilmiş satır düzenlenemez.");
                }

                var lineGross = line.Quantity * line.UnitPriceSnapshot;
                line.DiscountAmountSnapshot = Math.Min(discountAmount, lineGross);
                line.IsComplimentary = false;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (approverUserId is not null)
                {
                    await permissionService.LogApprovalAsync("ApplyOrderLineDiscount", performedByUserId, approverUserId, line.RestaurantOrder.RestaurantCheckId, line.Id, $"İndirim: {line.DiscountAmountSnapshot:N2}", cancellationToken);
                }

                return true;
            });
        }, cancellationToken);
    }

    // Mutfağa gönderilmiş bir satırda ikram durumunu aç/kapat - Edip, 2026-09-03: "mutfağa
    // gönderildi diye herşeyi pasif hale getirme, ikram düzeltme aktif olsun". İkram AÇILIRKEN
    // satırın brüt tutarı kadar indirim uygulanır (pending sepetteki İkram tuşuyla AYNI mantık),
    // KAPATILIRKEN indirim sıfırlanır.
    public Task ToggleLineComplimentaryAsync(
        int restaurantOrderLineId,
        string performedByUserId,
        string? approverPin = null,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                // Madde 9/21/25 - satır kilidi kaldırıldı, yetki kontrolü geçti. İkram kendi
                // başına ayrı bir kritik işlem (CanApplyComplimentary).
                if (!await permissionService.CanApplyComplimentaryAsync(performedByUserId, cancellationToken))
                {
                    throw new InvalidOperationException("İkram uygulama yetkiniz yok.");
                }

                string? approverUserId = null;
                if (await permissionService.RequiresSecondApprovalForComplimentaryAsync(cancellationToken))
                {
                    approverUserId = await permissionService.VerifyApproverPinAsync(approverPin, p => p.CanApplyComplimentary, cancellationToken)
                        ?? throw new InvalidOperationException("İkinci yetkili onayı gerekli - PIN geçersiz veya bu işlem için yetkisiz.");
                }

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var line = await dbContext.RestaurantOrderLines
                    .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck)
                    .SingleOrDefaultAsync(x => x.Id == restaurantOrderLineId, cancellationToken)
                    ?? throw new InvalidOperationException("Sipariş satırı bulunamadı.");

                if (line.RestaurantOrder.RestaurantCheck.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık adisyondaki satırlar düzenlenebilir.");
                }

                if (line.Status == RestaurantOrderLineStatus.Cancelled)
                {
                    throw new InvalidOperationException("İptal edilmiş satır düzenlenemez.");
                }

                line.IsComplimentary = !line.IsComplimentary;
                line.DiscountAmountSnapshot = line.IsComplimentary
                    ? line.Quantity * line.UnitPriceSnapshot
                    : 0;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (approverUserId is not null)
                {
                    await permissionService.LogApprovalAsync("ToggleLineComplimentary", performedByUserId, approverUserId, line.RestaurantOrder.RestaurantCheckId, line.Id, $"İkram: {line.IsComplimentary}", cancellationToken);
                }

                return true;
            });
        }, cancellationToken);

    // Vardiya aç - kasa (FinancialAccount) kullanıcı bazlı değil ŞUBE bazlı paylaşılır (Edip, 2026-08-08:
    // "kasa kullanıcı bazlı değil sube bazlı olucak... 5 tane kasıyer var hepsi nakıtlerı aynı kassaya
    // atablır") - yeni bir alan eklemek yerine ApplicationUser.DefaultFinancialAccountId zaten var olan
    // altyapı üzerinden yönetici tarafından aynı kasaya işaret edilerek kullanılır. Aynı kasada iki açık
    // vardiya olamaz (bkz. RestaurantCashShift unique filtered index).
    public Task<RestaurantCashShift> OpenShiftAsync(
        int financialAccountId,
        int branchId,
        string cashierUserId,
        decimal openingBalance,
        Guid? submissionKey,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                if (submissionKey is not null)
                {
                    var existing = await dbContext.RestaurantCashShifts
                        .SingleOrDefaultAsync(x => x.SubmissionKey == submissionKey, cancellationToken);
                    if (existing is not null)
                    {
                        return existing;
                    }
                }

                if (openingBalance < 0)
                {
                    throw new InvalidOperationException("Açılış tutarı negatif olamaz.");
                }

                var alreadyOpen = await dbContext.RestaurantCashShifts
                    .AnyAsync(x => x.FinancialAccountId == financialAccountId && x.Status == RestaurantCashShiftStatus.Open, cancellationToken);
                if (alreadyOpen)
                {
                    throw new InvalidOperationException("Bu kasada zaten açık bir vardiya var.");
                }

                var shift = new RestaurantCashShift
                {
                    CashierUserId = cashierUserId,
                    Status = RestaurantCashShiftStatus.Open,
                    OpenedAtUtc = DateTime.UtcNow,
                    OpeningBalance = openingBalance,
                    BranchId = branchId,
                    FinancialAccountId = financialAccountId,
                    SubmissionKey = submissionKey
                };
                dbContext.RestaurantCashShifts.Add(shift);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return shift;
            });
        }, cancellationToken);

    // Vardiya kapat - Beklenen Kapanış Bakiyesi sistem tarafından hesaplanır (açılış + vardiya
    // süresince bu kasaya (FinancialAccountId) yapılan iptal-olmayan tahsilatlar); kasiyerin
    // saydığı gerçek tutarla (ClosingBalanceCounted) karşılaştırma ekranda yapılır.
    public Task<RestaurantCashShift> CloseShiftAsync(
        int shiftId,
        decimal closingBalanceCounted,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var shift = await dbContext.RestaurantCashShifts
                    .SingleOrDefaultAsync(x => x.Id == shiftId, cancellationToken)
                    ?? throw new InvalidOperationException("Vardiya bulunamadı.");

                if (shift.Status != RestaurantCashShiftStatus.Open)
                {
                    throw new InvalidOperationException("Bu vardiya zaten kapatılmış.");
                }

                if (closingBalanceCounted < 0)
                {
                    throw new InvalidOperationException("Sayılan tutar negatif olamaz.");
                }

                var closedAt = DateTime.UtcNow;
                // Ters kayıtlar (bkz. CancelRetailSaleAsync) DAHİL edilir ama negatif işaretle -
                // aksi halde vardiya içinde iptal edilen bir fişin ödemesi beklenen kasa tutarında
                // hâlâ "alınmış" gibi sayılmaya devam ederdi.
                var cashInDuringShift = await dbContext.RestaurantPayments
                    .Where(x => x.FinancialAccountId == shift.FinancialAccountId
                        && x.PaidAtUtc >= shift.OpenedAtUtc
                        && x.PaidAtUtc <= closedAt)
                    .SumAsync(x => (decimal?)(x.IsReversal ? -x.Amount : x.Amount), cancellationToken) ?? 0m;

                shift.Status = RestaurantCashShiftStatus.Closed;
                shift.ClosedAtUtc = closedAt;
                shift.ClosingBalanceExpected = shift.OpeningBalance + cashInDuringShift;
                shift.ClosingBalanceCounted = closingBalanceCounted;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return shift;
            });
        }, cancellationToken);

    // Mutfak ekranı (KDS) - bir fiş TEK BÜTÜN olarak ilerletilir (Sent→InProgress→Ready→Served),
    // satır bazlı değil - gerçek mutfakta bir istasyona düşen sipariş toptan hazırlanır. İptal
    // edilmiş satırlar (KitchenTicketLineStatus.Cancelled) ilerletmeye dahil edilmez.
    // RestaurantOrderLine.Status bu ilerlemeden yeniden hesaplanır (bkz. §11 Karar 4).
    public Task<KitchenTicket> AdvanceKitchenTicketAsync(
        int kitchenTicketId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var ticket = await dbContext.KitchenTickets
                    .Include(x => x.Lines).ThenInclude(x => x.RestaurantOrderLine)
                    .SingleOrDefaultAsync(x => x.Id == kitchenTicketId, cancellationToken)
                    ?? throw new InvalidOperationException("Mutfak fişi bulunamadı.");

                var nextStatus = ticket.Status switch
                {
                    KitchenTicketStatus.Sent => KitchenTicketStatus.InProgress,
                    KitchenTicketStatus.InProgress => KitchenTicketStatus.Ready,
                    KitchenTicketStatus.Ready => KitchenTicketStatus.Served,
                    _ => throw new InvalidOperationException("Bu fiş zaten servis edilmiş.")
                };

                var nextLineStatus = nextStatus switch
                {
                    KitchenTicketStatus.InProgress => KitchenTicketLineStatus.InProgress,
                    KitchenTicketStatus.Ready => KitchenTicketLineStatus.Ready,
                    KitchenTicketStatus.Served => KitchenTicketLineStatus.Served,
                    _ => throw new InvalidOperationException("Beklenmeyen fiş durumu.")
                };

                var nextOrderLineStatus = nextStatus switch
                {
                    KitchenTicketStatus.InProgress => RestaurantOrderLineStatus.Preparing,
                    KitchenTicketStatus.Ready => RestaurantOrderLineStatus.Ready,
                    KitchenTicketStatus.Served => RestaurantOrderLineStatus.Served,
                    _ => throw new InvalidOperationException("Beklenmeyen fiş durumu.")
                };

                ticket.Status = nextStatus;
                foreach (var ticketLine in ticket.Lines.Where(x => x.Status != KitchenTicketLineStatus.Cancelled))
                {
                    ticketLine.Status = nextLineStatus;
                    if (ticketLine.RestaurantOrderLine.Status != RestaurantOrderLineStatus.Cancelled)
                    {
                        ticketLine.RestaurantOrderLine.Status = nextOrderLineStatus;
                    }
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ticket;
            });
        }, cancellationToken);

    // KitchenAutoReadyBackgroundService bunu periyodik çağırır - InventorySettings.
    // KitchenAutoReadyMinutes doluysa, gönderileli o süreden fazla geçmiş ama hâlâ Sent/InProgress
    // durumundaki fişleri AdvanceKitchenTicketAsync ile AYNI durum geçiş kurallarını izleyerek
    // (Sent→InProgress→Ready, tek adımda iki kez ilerletilerek) Hazır'a taşır - mutfak personeli
    // hiç dokunmasa bile (Edip, 2026-09-03). Zaten Ready/Served olan fişlere dokunulmaz.
    public async Task<int> AutoAdvanceOverdueKitchenTicketsAsync(CancellationToken cancellationToken = default)
    {
        var thresholdMinutes = await dbContext.InventorySettings
            .Where(x => x.Id == 1)
            .Select(x => x.KitchenAutoReadyMinutes)
            .SingleOrDefaultAsync(cancellationToken);

        if (thresholdMinutes is not > 0)
        {
            return 0;
        }

        var cutoffUtc = DateTime.UtcNow.AddMinutes(-thresholdMinutes.Value);
        var overdueTicketIds = await dbContext.KitchenTickets
            .Where(x => x.SentAtUtc <= cutoffUtc
                && (x.Status == KitchenTicketStatus.Sent || x.Status == KitchenTicketStatus.InProgress))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var advancedCount = 0;
        foreach (var ticketId in overdueTicketIds)
        {
            var ticket = await AdvanceKitchenTicketAsync(ticketId, cancellationToken);
            if (ticket.Status is KitchenTicketStatus.Sent or KitchenTicketStatus.InProgress)
            {
                await AdvanceKitchenTicketAsync(ticketId, cancellationToken);
            }
            advancedCount++;
        }

        return advancedCount;
    }

    // Faz 3: adisyon kapanışı - ödeme alma + RetailSale/cari/finansal hareket postalama.
    // Fiyatlar KDV DAHİL tutulduğu için (bkz. RestaurantPricingCalculator) matrah/KDV ayrımı
    // SADECE burada, kapanış anında yapılır. Kural (Edip, 2026-08-07): müşteri ayrıca
    // yakalanmadıysa (walk-in) satış sabit "Perakende Satışlar Carisi" cariye ve "Perakende
    // yurtiçi ticaret" ticaret türüne postalanır - bkz. RetailSale.TradeType. Ödeme anında tam
    // tahsil edildiği varsayılır (Kapalı Fatura'daki gibi): Satış hareketi (Borç) hemen ardından
    // Tahsilat hareketi (Alacak) ile aynı tutarda nötrlenir, cari üzerinde bakiye birikmez.
    public Task<RetailSale> CloseCheckAsync(
        int checkId,
        IReadOnlyList<RestaurantPaymentInput> payments,
        int? customerId,
        string closedByUserId,
        Guid submissionKey,
        FiscalReceiptInfo? fiscalInfo = null,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                // GERÇEK KÖK NEDEN (2026-09-07, Z Dönemi P0 eşzamanlılık testinde bulundu) -
                // EnableRetryOnFailure (Program.cs) hem bu execution strategy'yi HEM DE
                // ExecuteWithConcurrencyRetryAsync'in dış tekrar denemesini kullanıyor; her ikisi
                // de AYNI dbContext örneğini tekrar dener. Bir önceki deneme transaction'ı SQL
                // tarafında rollback olsa bile dbContext.ChangeTracker rollback ile OTOMATİK
                // temizlenmez - yarım kalan Add()'lenmiş/Include ile yüklenmiş tracked entity'ler
                // context'te asılı kalır. Sonraki deneme aynı check'i SingleOrDefaultAsync ile
                // sorgularken, EF'in identity-map eşlemesi bu tutarsız tracked kaydı GERÇEK SQL
                // sonucuyla birleştiremiyor ve "Sequence contains no elements" ile patlıyor (12
                // eşzamanlı gerçek CloseCheckAsync çağrısıyla doğrulandı: 10/12 başarısız, tekrar
                // deneme öncesi Clear() eklenince 12/12 başarılı - bkz. commit mesajı). Microsoft'un
                // kendi Connection Resiliency dokümantasyonu da tam bunu öneriyor: "reset the state
                // of the context before retrying". Clear() ucuzdur (yalnızca in-memory tracking'i
                // sıfırlar, DB'ye dokunmaz) - her retry biriminin EN BAŞINDA çağrılmalı.
                dbContext.ChangeTracker.Clear();

                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var check = await dbContext.RestaurantChecks
                    .Include(x => x.Orders).ThenInclude(x => x.Lines).ThenInclude(x => x.Product)
                    .Include(x => x.RestaurantTableSession)
                    .SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
                    ?? throw new InvalidOperationException("Adisyon bulunamadı.");

                // Çift tıklama/mükerrer POST koruması.
                if (check.SubmissionKey == submissionKey && check.Status == RestaurantCheckStatus.Closed)
                {
                    return await dbContext.RetailSales.SingleAsync(x => x.RestaurantCheckId == check.Id, cancellationToken);
                }

                if (check.Status != RestaurantCheckStatus.Open)
                {
                    throw new InvalidOperationException("Bu adisyon zaten kapalı veya iptal edilmiş.");
                }

                // "Cari Ekle" (madde 13) ile önceden bağlanmış bir müşteri varsa ve istemci ayrıca
                // bir customerId göndermediyse (fatura kesme senaryosu değilse) onu kullan - Açık
                // Hesap ödeme yöntemi için bu ZORUNLU kaynaktır.
                customerId ??= check.AttachedCustomerId;

                var lines = check.Orders
                    .SelectMany(o => o.Lines)
                    .Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .ToList();
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException("Boş adisyon kapatılamaz.");
                }

                // GERÇEK HATA (2026-09-06, kabul testinde bulundu, Fiş İkram) - burada eskiden
                // "payments.Count == 0" koşulsuz reddediliyordu. Tam İkram edilmiş bir adisyonda
                // (tüm satırlar %100 indirimli, grandTotal=0) toplanacak HİÇBİR tutar yoktur - sıfır
                // ödeme satırıyla kapatmak GEÇERLİ bir durumdur. Bu erken kontrol tamamen kaldırıldı;
                // aşağıdaki "paymentsTotal != grandTotal" kontrolü zaten AYNI korumayı (grandTotal>0
                // iken payments boşsa 0 != grandTotal olur, reddeder) daha isabetli bir hata
                // mesajıyla sağlıyor.
                decimal subtotal = 0, tax = 0, discount = 0, grossLinesTotal = 0;
                var retailSaleLines = new List<RetailSaleLine>();
                var lineTotals = new List<(RestaurantOrderLine Line, decimal LineTotal)>();
                foreach (var line in lines)
                {
                    var lineTotal = Math.Round(line.Quantity * line.UnitPriceSnapshot - line.DiscountAmountSnapshot, 2, MidpointRounding.AwayFromZero);
                    grossLinesTotal += lineTotal;
                    discount += line.DiscountAmountSnapshot;
                    lineTotals.Add((line, lineTotal));

                    retailSaleLines.Add(new RetailSaleLine
                    {
                        ProductNameSnapshot = line.ProductNameSnapshot,
                        Quantity = line.Quantity,
                        UnitPriceSnapshot = line.UnitPriceSnapshot,
                        TaxRateSnapshot = line.TaxRateSnapshot,
                        DiscountAmountSnapshot = line.DiscountAmountSnapshot,
                        LineTotal = lineTotal,
                        ProductId = line.ProductId,
                        IsComplimentary = line.IsComplimentary
                    });
                }

                // Adisyon indirimi (TicketDiscountAmount) BASILAN satırlara/RetailSaleLine'lara
                // YAZILMAZ (2026-09-05 teknik doküman madde 4) - ama KDV matrahı doğru çıksın diye
                // yalnızca bu döngüde, her satırın payına göre orantılı olarak GEÇİCİ düşülür.
                var ticketDiscount = check.TicketDiscountAmount;
                foreach (var (line, lineTotal) in lineTotals)
                {
                    var share = grossLinesTotal > 0 ? lineTotal / grossLinesTotal : 0;
                    var taxableLineTotal = lineTotal - Math.Round(ticketDiscount * share, 2, MidpointRounding.AwayFromZero);
                    var (matrah, kdvTutari) = RestaurantPricingCalculator.ExtractTax(taxableLineTotal, line.TaxRateSnapshot);
                    subtotal += matrah;
                    tax += kdvTutari;
                }

                var grandTotal = Math.Round(grossLinesTotal, 2, MidpointRounding.AwayFromZero) - ticketDiscount;
                discount += ticketDiscount;
                var paymentsTotal = Math.Round(payments.Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);
                if (paymentsTotal != grandTotal)
                {
                    throw new InvalidOperationException($"Ödeme toplamı ({paymentsTotal:N2}) adisyon tutarına ({grandTotal:N2}) eşit değil.");
                }

                // Ödenmez (madde 12) - ürün/tutar satılmış SAYILIR (RetailSale.GrandTotal, satır
                // toplamları değişmez - fiş üzerinde tam görünür), ama fiilen tahsil edilmediği
                // için hem Sale (ciro) hem Collection (tahsilat) muhasebe kayıtlarından NET
                // OLARAK dışarıda bırakılır - cari hesap dengede kalır (Debit==Credit), gerçek
                // gelir/tahsilat asla oluşmaz. İkram'dan (satır fiyatının kendisi sıfırlanır)
                // FARKI budur.
                var unpaidTotal = Math.Round(payments.Where(p => p.Method == RestaurantPaymentMethod.Unpaid).Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);

                // Açık Hesap (madde 13) - Ödenmez'den FARKI: Sale (ciro) NORMAL oluşur, sadece
                // Collection (tahsilat) oluşmaz - seçilen CARİNİN hesabında gerçek bir açık
                // alacak bırakır. Cari seçimi ZORUNLUDUR (spec: "Cari seçmelisiniz." uyarısı).
                var openAccountTotal = Math.Round(payments.Where(p => p.Method == RestaurantPaymentMethod.OpenAccount).Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);
                if (openAccountTotal > 0 && customerId is null)
                {
                    throw new InvalidOperationException("Cari seçmelisiniz.");
                }

                var netSaleTotal = grandTotal - unpaidTotal;
                var netCollectionTotal = grandTotal - unpaidTotal - openAccountTotal;

                var effectiveCustomerId = customerId ?? await GetDefaultRetailCustomerIdAsync(cancellationToken);
                const string tradeType = "Perakende yurtiçi ticaret";
                var documentNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("RETAIL_SALE", cancellationToken);

                var retailSale = new RetailSale
                {
                    DocumentNumber = documentNumber,
                    Status = RetailSaleStatus.Issued,
                    IssuedAtUtc = DateTime.UtcNow,
                    SubtotalAmount = subtotal,
                    DiscountAmount = discount,
                    ServiceChargeAmount = 0,
                    TaxAmount = tax,
                    GrandTotal = grandTotal,
                    TradeType = tradeType,
                    CustomerId = effectiveCustomerId,
                    RestaurantCheck = check,
                    Lines = retailSaleLines,
                    FiscalDeviceSerialNumber = fiscalInfo?.DeviceSerialNumber,
                    FiscalReceiptNumber = fiscalInfo?.ReceiptNumber,
                    ZReportNumber = fiscalInfo?.ZNo,
                    FiscalizationStatus = fiscalInfo?.ReceiptNumber is not null
                        ? RetailSaleFiscalizationStatus.Fiscalized
                        : RetailSaleFiscalizationStatus.NotFiscalized
                };
                dbContext.RetailSales.Add(retailSale);

                var closedByUserBranchId = await dbContext.Users
                    .Where(x => x.Id == closedByUserId)
                    .Select(x => x.BranchId)
                    .SingleOrDefaultAsync(cancellationToken);

                // Z Dönem Kapatma test talimatı (2026-09-06, Edip: "bir satışın hangi Z'ye ait
                // olduğu sonradan tahmin edilmemeli - satış finansal olarak kapanırken içinde
                // bulunduğu aktif Z dönemiyle ilişkilendirilmeli") - satış GERÇEKTEN kapanırken,
                // burada, o anki aktif (Status=Open) Z dönemine kalıcı FK ile bağlanır. Yoksa
                // (ilk satış / önceki Z henüz kapatılmamışsa bu asla olmaz ama ilk kurulumda hiç
                // dönem yoksa) biri burada lazy oluşturulur.
                var effectiveBranchId = closedByUserBranchId
                    ?? await dbContext.Branches.Where(x => x.IsHeadOffice).Select(x => x.Id).FirstAsync(cancellationToken);
                retailSale.RestaurantZPeriodId = await GetOrCreateActiveZPeriodIdAsync(effectiveBranchId, cancellationToken);

                // Stok hareketi (2026-09-05, Edip: kritik kabul testi bulgusu + talimatı) - Self/
                // Masa/Paket TÜM ödeme türlerinde (Nakit/Kart/Açık Hesap/Ödenmez/İkram) adisyon
                // GERÇEKTEN kapanınca, burada TEK bir choke point'te, normal Fatura/İrsaliye/Stok
                // Fişi akışlarındaki AYNI StockMovement+Product.StockQuantity deseni kullanılır
                // (bkz. InvoicePostingService.PostStockAndAccountAsync) - ayrı/geçici bir stok
                // sistemi İCAT EDİLMEDİ. "Ödeme türü stok hareketini değiştirmemeli" (Edip) -
                // netSaleTotal/netCollectionTotal ayrımı burada YOK, tüm iptal edilmemiş satırlar
                // aynı şekilde düşülür. Açık adisyon üzerindeki satır/adisyon iptalinde
                // (CancelOrderLineAsync/VoidEmptyCheckAsync) hiç stok hareketi oluşmadığından
                // (henüz satış kapanmadı - Edip'in "finansal posting yapılmasın" kuralıyla
                // simetrik) burada "ters" bir şeye gerek yok, iptal edilen satırlar zaten
                // yukarıdaki `lines` filtresinde (Status != Cancelled) hiç yer almıyor.
                var inventorySettings = await dbContext.InventorySettings
                    .Where(x => x.Id == 1)
                    .SingleOrDefaultAsync(cancellationToken)
                    ?? throw new InvalidOperationException("Envanter ayarları bulunamadı.");
                var trackedLines = lines.Where(x => x.Product.TrackStock).ToList();
                if (trackedLines.Count > 0)
                {
                    var warehouse = await dbContext.Warehouses
                        .Where(x => x.IsActive && (closedByUserBranchId == x.BranchId || x.Branch.IsHeadOffice))
                        .OrderByDescending(x => closedByUserBranchId == x.BranchId)
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new InvalidOperationException("Stok hareketi için aktif bir depo bulunamadı.");

                    foreach (var line in trackedLines)
                    {
                        if (inventorySettings.EnforceStockLevel &&
                            !inventorySettings.AllowNegativeStock &&
                            !inventorySettings.AllowSaleWhenOutOfStock)
                        {
                            var available = await inventoryBalance.GetAvailableAsync(
                                line.ProductId, null, warehouse.Id, cancellationToken);
                            if (available < line.Quantity)
                            {
                                throw new InvalidOperationException(
                                    $"{line.ProductNameSnapshot} için yeterli stok yok. Mevcut: {available:N3}");
                            }
                        }

                        dbContext.StockMovements.Add(new StockMovement
                        {
                            MovementDateUtc = DateTime.UtcNow,
                            MovementType = StockMovementType.Sale,
                            Quantity = -line.Quantity,
                            UnitCost = 0,
                            DocumentNumber = documentNumber,
                            ProductId = line.ProductId,
                            WarehouseId = warehouse.Id,
                            RestaurantOrderLineId = line.Id,
                            Description = $"Restoran satışı - {check.CheckNumber}"
                        });
                        line.Product.StockQuantity -= line.Quantity;
                    }
                }

                // netSaleTotal (Ödenmez hariç, Açık Hesap DAHİL - revenue Açık Hesap'ta da
                // gerçekleşir) > 0 ise Sale hareketi oluşur. netCollectionTotal (Ödenmez VE Açık
                // Hesap hariç) > 0 ise Collection hareketi oluşur - tamamı Açık Hesap ise Sale
                // oluşur ama Collection oluşmaz (gerçek açık alacak), tamamı Ödenmez ise İKİSİ DE
                // oluşmaz.
                CurrentAccountTransaction? collectionAccountTransaction = null;
                if (netSaleTotal > 0)
                {
                    dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = CurrentAccountTransactionType.Sale,
                        DocumentNumber = documentNumber,
                        CurrencyCode = "TRY",
                        ExchangeRate = 1,
                        Debit = netSaleTotal,
                        Credit = 0,
                        CustomerId = effectiveCustomerId,
                        Description = $"Restoran satışı - {check.CheckNumber}"
                    });
                }

                if (netCollectionTotal > 0)
                {
                    collectionAccountTransaction = new CurrentAccountTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = CurrentAccountTransactionType.Collection,
                        DocumentNumber = documentNumber,
                        CurrencyCode = "TRY",
                        ExchangeRate = 1,
                        Debit = 0,
                        Credit = netCollectionTotal,
                        CustomerId = effectiveCustomerId,
                        Description = $"Restoran tahsilatı - {check.CheckNumber}"
                    };
                    dbContext.CurrentAccountTransactions.Add(collectionAccountTransaction);
                }

                foreach (var payment in payments)
                {
                    // Ödenmez/Açık Hesap satırı - RestaurantPayment kaydı (izlenebilirlik/raporlama)
                    // OLUŞUR, ama gerçek bir tahsilat hareketi olmadığı için FinancialTransaction
                    // HİÇ yazılmaz (Açık Hesap'ta tutar carinin AÇIK ALACAĞI olarak kalır - Sale
                    // hareketi zaten yukarıda oluştu, burada sadece kasa/banka hareketi engellenir).
                    if (payment.Method is RestaurantPaymentMethod.Unpaid or RestaurantPaymentMethod.OpenAccount)
                    {
                        dbContext.RestaurantPayments.Add(new RestaurantPayment
                        {
                            PaymentMethod = payment.Method,
                            Amount = payment.Amount,
                            PaidAtUtc = DateTime.UtcNow,
                            RestaurantCheck = check,
                            FinancialAccountId = payment.FinancialAccountId,
                            SubmissionKey = submissionKey,
                            FinancialTransaction = null
                        });
                        continue;
                    }

                    var financialTransaction = new FinancialTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = FinancialTransactionType.Collection,
                        DocumentNumber = documentNumber,
                        Amount = payment.Amount,
                        ExchangeRate = 1,
                        Description = $"Restoran tahsilatı - {check.CheckNumber}",
                        FinancialAccountId = payment.FinancialAccountId,
                        CustomerId = effectiveCustomerId,
                        CurrentAccountTransaction = collectionAccountTransaction
                    };
                    dbContext.FinancialTransactions.Add(financialTransaction);

                    dbContext.RestaurantPayments.Add(new RestaurantPayment
                    {
                        PaymentMethod = payment.Method,
                        Amount = payment.Amount,
                        PaidAtUtc = DateTime.UtcNow,
                        RestaurantCheck = check,
                        FinancialAccountId = payment.FinancialAccountId,
                        SubmissionKey = submissionKey,
                        FinancialTransaction = financialTransaction
                    });
                }

                check.Status = RestaurantCheckStatus.Closed;
                check.ClosedAtUtc = DateTime.UtcNow;
                check.SubmissionKey = submissionKey;
                check.SubtotalAmount = subtotal;
                check.DiscountAmount = discount;
                check.TaxAmount = tax;
                check.GrandTotal = grandTotal;
                check.LinkedRetailSale = retailSale;

                // Split-check altyapısı ileride devreye girerse aynı oturumda başka açık adisyon
                // kalmışsa oturumu kapatma - bkz. §11 Karar 5.
                var hasOtherOpenChecks = await dbContext.RestaurantChecks.AnyAsync(
                    x => x.RestaurantTableSessionId == check.RestaurantTableSessionId
                        && x.Id != check.Id
                        && x.Status == RestaurantCheckStatus.Open,
                    cancellationToken);
                if (!hasOtherOpenChecks)
                {
                    check.RestaurantTableSession.Status = RestaurantTableSessionStatus.Closed;
                    check.RestaurantTableSession.ClosedAtUtc = DateTime.UtcNow;
                }

                // Hibrit senkron (Faz C): şube bu olayı merkeze göndermek üzere kuyruğa alır.
                // RetailSale.RecordId veritabanında NEWSEQUENTIALID() ile üretiliyor (bkz.
                // ApplicationDbContext.OnModelCreating, ValueGeneratedOnAdd) - EF Core bu değeri
                // ancak INSERT gerçekleştikten SONRA (OUTPUT ile) belleğe okur. Bu yüzden payload
                // burada kurulmadan önce retailSale'i tek başına flush ediyoruz (Talimat 1 Section
                // 11 mutabakat testinde bulundu: 302/303 kayıtta RetailSaleRecordId sıfır GUID
                // çıkıyordu - transaction henüz commit edilmediği için atomiklik bozulmuyor, aynı
                // transaction içinde aşağıdaki SaveChangesAsync+CommitAsync ile birlikte tek bir
                // bütün olarak ya tamamen işleniyor ya da tamamen geri alınıyor).
                await dbContext.SaveChangesAsync(cancellationToken);

                dbContext.IntegrationOutboxMessages.Add(new IntegrationOutboxMessage
                {
                    EventType = "RestaurantCheckClosed",
                    PayloadJson = JsonSerializer.Serialize(new RestaurantCheckClosedPayload
                    {
                        RetailSaleRecordId = retailSale.RecordId,
                        DocumentNumber = retailSale.DocumentNumber,
                        IssuedAtUtc = retailSale.IssuedAtUtc,
                        SubtotalAmount = retailSale.SubtotalAmount,
                        TaxAmount = retailSale.TaxAmount,
                        GrandTotal = retailSale.GrandTotal,
                        TradeType = retailSale.TradeType,
                        CheckNumber = check.CheckNumber,
                        RestaurantZPeriodId = retailSale.RestaurantZPeriodId,
                        RestaurantZNo = retailSale.RestaurantZPeriodId is { } zId ? $"Z-{zId:D6}" : null
                    })
                });

                // Kısmi ödeme kayıtları (madde 4-8) - kapanışla birlikte artık ANLAMSIZ (gerçek
                // RestaurantPayment satırları az önce yukarıda yazıldı), muhasebeye HİÇ İŞLENMEMİŞ
                // oldukları için silinmeleri güvenli - bkz. RestaurantCheckPendingPayment yorumu.
                var pendingPayments = await dbContext.RestaurantCheckPendingPayments
                    .Where(x => x.RestaurantCheckId == checkId)
                    .ToListAsync(cancellationToken);
                dbContext.RestaurantCheckPendingPayments.RemoveRange(pendingPayments);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Çıktı Tasarımcısı entegrasyonu (kademeli geçiş, adım 4a: Adisyon Fişi) - satış
                // akışını ASLA bloklamaz (bkz. PrintDispatchService), bu yüzden transaction
                // KAPANDIKTAN SONRA, best-effort olarak çağrılır. Bu role atanmış aktif yazıcı
                // yoksa sessizce atlanır - eski window.print() akışı (Receipt.cshtml) HİÇ
                // DOKUNULMADAN paralel çalışmaya devam eder.
                try
                {
                    await printDispatchService.EnqueueForRoleAsync(
                        Domain.Enums.PrinterRole.Adisyon,
                        effectiveBranchId,
                        Domain.Enums.PrintTemplateType.Adisyon,
                        retailSale.Id,
                        $"Adisyon kapanışı - {retailSale.DocumentNumber}",
                        cancellationToken);
                }
                catch
                {
                    // Yazdırma kuyruğa alınamasa bile satış zaten KAPANDI ve kalıcı - burada
                    // fırlatmak satışı geri almaz (transaction zaten commit edildi), sadece
                    // kullanıcıya gereksiz bir hata gösterirdi.
                }

                return retailSale;
            });
        }, cancellationToken);

    // Z Dönem Kapatma test talimatı (2026-09-06) - RestaurantCashShift'ten (Vardiya) BİLEREK ayrı
    // tutulur (bkz. RestaurantZPeriod.cs yorumu). Bu metod ÇAĞRILDIĞI transaction'ın İÇİNDE
    // çalışır (CloseCheckAsync zaten Serializable bir transaction açmış durumda) - ayrı bir
    // transaction/strategy AÇMAZ, sadece dbContext üzerinde okuma/ekleme yapar.
    private async Task<int> GetOrCreateActiveZPeriodIdAsync(int branchId, CancellationToken cancellationToken)
    {
        var openPeriodId = await dbContext.RestaurantZPeriods
            .Where(x => x.Status == RestaurantZPeriodStatus.Open)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync(cancellationToken);
        if (openPeriodId is not null)
        {
            return openPeriodId.Value;
        }

        var newPeriod = new RestaurantZPeriod
        {
            Status = RestaurantZPeriodStatus.Open,
            OpenedAtUtc = DateTime.UtcNow,
            BranchId = branchId
        };
        dbContext.RestaurantZPeriods.Add(newPeriod);
        await dbContext.SaveChangesAsync(cancellationToken);
        return newPeriod.Id;
    }

    // "Z Raporu Al" (2026-09-06, mevcut CreateDirectZReportAsync/RestaurantCashShift tabanlı
    // mekanizmanın YERİNİ alır - o mekanizma zaman-aralığı tahminiyle çalışıyordu, Edip'in ek
    // talimatı ("sonradan tahmin edilmemeli") bunu GERÇEK bir FK ilişkisine taşımayı gerektirdi.
    // Aktif Z dönemini kapatır (LINKED satışlardan - zaman aralığından DEĞİL - özet hesaplanıp
    // donar), hemen ardından yeni bir Açık dönem başlatır. Muhasebe/kasa/banka/cari/stok hiç
    // etkilenmez - bu yalnızca restoran tarafının "aktif dönem" özetleyici/kapatıcısıdır, ikinci
    // kez hiçbir hareket post etmez (madde: "Z özetleyici/kapatıcıdır, satış hareketlerini ikinci
    // kez post etmez").
    public Task<RestaurantZPeriod> CloseActiveZPeriodAsync(
        string? closedByUserId,
        CancellationToken cancellationToken = default,
        bool isAutomatic = false) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var openPeriod = await dbContext.RestaurantZPeriods
                    .SingleOrDefaultAsync(x => x.Status == RestaurantZPeriodStatus.Open, cancellationToken)
                    ?? throw new InvalidOperationException("Açık bir Z dönemi bulunamadı.");

                var sales = await dbContext.RetailSales
                    .Where(x => x.RestaurantZPeriodId == openPeriod.Id && x.Status != RetailSaleStatus.Cancelled)
                    .Select(x => new { x.SubtotalAmount, x.DiscountAmount, x.TaxAmount, x.GrandTotal, x.RestaurantCheckId })
                    .ToListAsync(cancellationToken);

                openPeriod.ReceiptCount = sales.Count;
                openPeriod.GrossTotal = sales.Sum(x => x.SubtotalAmount + x.TaxAmount + x.DiscountAmount);
                openPeriod.TaxTotal = sales.Sum(x => x.TaxAmount);

                // Ödenmez iş kuralı (2026-09-06, Edip) - Ödenmez'in finansal karşılığı (kasa/banka/
                // cari) yoktur, İkram gibi Net Ciro'ya DAHİL EDİLMEZ; ürün müşteriye çıktığı için fiş/
                // Z bağlantısı ve stok hareketi korunur, yalnızca ciro tutarından düşülür. İkram ile
                // ASLA birleştirilmez (İkram = RestaurantOrderLine.IsComplimentary/ComplimentaryTotal,
                // ayrı bir alan; Ödenmez = RestaurantPayment.PaymentMethod=Unpaid, kendi payment-
                // breakdown satırında ayrıca gösterilir). Bu tek kural Dashboard/X Raporu/Kasiyer
                // Raporu'nda da (RestaurantDashboardController/RestaurantReportsController) aynı
                // şekilde uygulanır.
                var unpaidTotal = sales.Count == 0
                    ? 0m
                    : await dbContext.RestaurantPayments
                        .Where(x => sales.Select(s => s.RestaurantCheckId).Contains(x.RestaurantCheckId)
                            && !x.IsReversal && x.PaymentMethod == RestaurantPaymentMethod.Unpaid)
                        .SumAsync(x => x.Amount, cancellationToken);
                openPeriod.NetTotal = sales.Sum(x => x.GrandTotal) - unpaidTotal;

                if (sales.Count > 0)
                {
                    // GERÇEK HATA (2026-09-06, Z kapatma kabul testinde bulundu) - ComplimentaryTotal
                    // burada RestaurantOrderLines.DiscountAmountSnapshot'tan (satır bazlı İkram)
                    // doğru hesaplanıyordu, ama DiscountTotal AYNI kaynaktan okunuyordu - fiş
                    // TOPLAMINA uygulanan tutar/yüzde indirimi (ApplyTicketDiscountAsync, bkz.
                    // restaurant-pos.js "toplam tutara indirim") satırlara HİÇ dağıtılmaz, sadece
                    // RestaurantCheck.TicketDiscountAmount'a yazılır - bu yüzden ticket-seviyesi
                    // indirimler Z özetinde SESSİZCE 0 görünüyordu. RetailSale.DiscountAmount HER
                    // İKİ türü de (ticket + satır, İkram dahil) zaten doğru topluyor - gerçek
                    // DiscountTotal, bu toplamdan İkram payını (ComplimentaryTotal) çıkararak
                    // bulunur.
                    var checkIds = sales.Select(x => x.RestaurantCheckId).ToList();
                    var complimentaryTotal = await dbContext.RestaurantOrderLines
                        .Where(x => checkIds.Contains(x.RestaurantOrder.RestaurantCheckId) && x.Status != RestaurantOrderLineStatus.Cancelled && x.IsComplimentary)
                        .SumAsync(x => x.DiscountAmountSnapshot, cancellationToken);
                    openPeriod.ComplimentaryTotal = complimentaryTotal;
                    openPeriod.DiscountTotal = sales.Sum(x => x.DiscountAmount) - complimentaryTotal;
                }

                openPeriod.Status = RestaurantZPeriodStatus.Closed;
                openPeriod.ClosedAtUtc = DateTime.UtcNow;
                openPeriod.ClosedByUserId = closedByUserId;
                openPeriod.ClosedAutomatically = isAutomatic;

                var newPeriod = new RestaurantZPeriod
                {
                    Status = RestaurantZPeriodStatus.Open,
                    OpenedAtUtc = openPeriod.ClosedAtUtc.Value,
                    BranchId = openPeriod.BranchId
                };
                dbContext.RestaurantZPeriods.Add(newPeriod);

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return openPeriod;
            });
        }, cancellationToken);

    // Otomatik Z (Talimat 1, 2026-09-06) - RestaurantAutoZBackgroundService'in her ~30 saniyede
    // bir çağırdığı kontrol. Talimat'ın kendi sözleriyle: "Sistem 23:59'da 'bugün Z alındı mı?'
    // diye körlemesine BAKMAYACAKTIR. Esas kontrol: o anda içerisinde finansal olarak kapanmış
    // satış bulunan ve henüz Z ile kapatılmamış aktif RestaurantZPeriod var mı?" - bilerek GÜN
    // BAZLI BİR WATERMARK YOK (ilk sürümde vardı, kaldırıldı - spesifikasyona aykırıydı ve gerçek
    // testte tam da öngörülen şekilde yanlış davrandı: dönem boşken saat sınırı geçildiğinde
    // watermark günü "işlendi" say diye kilitliyor, o günün İÇİNDE sonradan gelen satış hiç
    // otomatik Z'ye giremiyordu). Doğal kendi-kendini-sınırlama zaten yeterli: bir dönem kapanıp
    // hemen boş yeniden açıldığında (CloseActiveZPeriodAsync), o yeni dönem BOŞ olduğu için bir
    // sonraki pollde tekrar kapanmaz - yeni bir satış gelene kadar. Aynı gün içinde birden fazla
    // otomatik Z alınması KASITLI ve BEKLENEN bir davranıştır (talimat: "esas kavram takvim günü
    // değil Z dönemidir"). Çift kapanma/duplicate Z riski watermark'a değil, tek-açık-dönem unique
    // index'ine + CloseActiveZPeriodAsync'in Serializable transaction'ına dayanır: aynı anda iki
    // çağrı gelse bile ikisi de AYNI açık dönemi hedefler, biri kapatır, öteki ya serileştirme
    // çakışmasıyla yeniden dener (ve o noktada dönem zaten kapanmış/yeni dönem boş olduğu için
    // hiçbir şey yapmaz) ya da zaten boş bulur - MANUEL Z ile TAMAMEN AYNI motoru kullanır, ayrı
    // bir "otomatik kapanış" mantığı YOK.
    public async Task<RestaurantZPeriod?> RunAutomaticZCheckAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.InventorySettings
            .AsNoTracking()
            .SingleAsync(x => x.Id == 1, cancellationToken);
        if (!settings.AutoZEnabled || settings.AutoZTimeLocal is null)
        {
            return null;
        }

        var nowLocal = nowUtc.ToLocalTime();
        if (nowLocal.TimeOfDay < settings.AutoZTimeLocal.Value)
        {
            return null;
        }

        var openPeriod = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Status == RestaurantZPeriodStatus.Open, cancellationToken);
        if (openPeriod is null)
        {
            return null;
        }

        var hasClosedSales = await dbContext.RetailSales
            .AnyAsync(x => x.RestaurantZPeriodId == openPeriod.Id && x.Status != RetailSaleStatus.Cancelled, cancellationToken);
        if (!hasClosedSales)
        {
            return null;
        }

        return await CloseActiveZPeriodAsync(closedByUserId: null, cancellationToken, isAutomatic: true);
    }

    // Kısmi ödeme (madde 4-8) - "Ödemeyi Al" modalında bir yöntem tuşuna basılınca sunucuya
    // KALICI olarak kaydedilir (RestaurantPayment/FinancialTransaction DEĞİL - muhasebeye henüz
    // hiç dokunmaz). Bu sayede "Adisyona Dön" ile modal kapatılıp tekrar açıldığında, hatta sayfa
    // yenilense/başka bir kullanıcı aynı adisyonu açsa bile alınan tutarlar KAYBOLMAZ (spec:
    // "kısmi ödemeler korunur, ana ekranda da Ödenen/Kalan görünür").
    public async Task<RestaurantCheckPendingPayment> AddPendingPaymentAsync(
        int checkId,
        RestaurantPaymentMethod method,
        decimal amount,
        int? financialAccountId,
        string recordedByUserId,
        IReadOnlyCollection<int>? settleOrderLineIds = null,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException("Tutar sıfırdan büyük olmalıdır.");
        }

        var check = await dbContext.RestaurantChecks.SingleOrDefaultAsync(x => x.Id == checkId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");
        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Yalnızca açık adisyonlara ödeme kaydedilebilir.");
        }

        var pending = new RestaurantCheckPendingPayment
        {
            RestaurantCheckId = checkId,
            PaymentMethod = method,
            Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero),
            FinancialAccountId = financialAccountId,
            RecordedByUserId = recordedByUserId,
            RecordedAtUtc = DateTime.UtcNow
        };
        dbContext.RestaurantCheckPendingPayments.Add(pending);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Ürün bazlı tahsilat (Edip, 2026-09-28: "ödemesi alınan ürünleri ekranda üzerine çizgi
        // çeksin ve bir daha işlem yaptırmasın") - bu ödeme seçilen ürünler için alındıysa, o
        // satırlar bu ödeme kaydına bağlanır. Sıradan (ürün seçilmemiş) bir kısmi ödemede bu
        // parametre boş gelir, HİÇBİR satır etkilenmez - mevcut yazma mantığı AYNEN kalır.
        if (settleOrderLineIds is { Count: > 0 })
        {
            var linesToSettle = await dbContext.RestaurantOrderLines
                .Where(x => settleOrderLineIds.Contains(x.Id)
                    && x.RestaurantOrder.RestaurantCheckId == checkId
                    && x.SettledByPendingPaymentId == null)
                .ToListAsync(cancellationToken);
            foreach (var line in linesToSettle)
            {
                line.SettledByPendingPaymentId = pending.Id;
                line.UpdatedAtUtc = DateTime.UtcNow;
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return pending;
    }

    public async Task RemovePendingPaymentAsync(int pendingPaymentId, CancellationToken cancellationToken = default)
    {
        var pending = await dbContext.RestaurantCheckPendingPayments.SingleOrDefaultAsync(x => x.Id == pendingPaymentId, cancellationToken)
            ?? throw new InvalidOperationException("Ödeme satırı bulunamadı.");

        // Bu ödemenin "ödendi" işaretlediği ürünler varsa (ürün bazlı tahsilat), ödeme geri
        // alınınca onlar da tekrar normale döner (üzeri çizili kalmaz, tekrar işlem yapılabilir).
        var settledLines = await dbContext.RestaurantOrderLines
            .Where(x => x.SettledByPendingPaymentId == pendingPaymentId)
            .ToListAsync(cancellationToken);
        foreach (var line in settledLines)
        {
            line.SettledByPendingPaymentId = null;
            line.UpdatedAtUtc = DateTime.UtcNow;
        }

        dbContext.RestaurantCheckPendingPayments.Remove(pending);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // "Ödeme İptal" (madde 7) - bu adisyondaki TÜM kısmi ödemeleri sıfırlar. "Adisyona Dön"den
    // (madde 8) FARKI: kullanıcı ödeme ekranında KALIR, hiçbir şey korunmaz - buradaki tek
    // sorumluluk sunucudaki kısmi ödeme kayıtlarını silmek, ekran akışı JS tarafında yönetilir.
    public async Task CancelPendingPaymentsAsync(int checkId, CancellationToken cancellationToken = default)
    {
        var pendingPayments = await dbContext.RestaurantCheckPendingPayments
            .Where(x => x.RestaurantCheckId == checkId)
            .ToListAsync(cancellationToken);

        // RemovePendingPaymentAsync'teki AYNI kural - toplu iptalde de ürün bazlı tahsilatla
        // "ödendi" işaretlenmiş satırlar tekrar normale döner.
        var pendingIds = pendingPayments.Select(x => x.Id).ToList();
        var settledLines = await dbContext.RestaurantOrderLines
            .Where(x => x.SettledByPendingPaymentId != null && pendingIds.Contains(x.SettledByPendingPaymentId!.Value))
            .ToListAsync(cancellationToken);
        foreach (var line in settledLines)
        {
            line.SettledByPendingPaymentId = null;
            line.UpdatedAtUtc = DateTime.UtcNow;
        }

        dbContext.RestaurantCheckPendingPayments.RemoveRange(pendingPayments);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> GetDefaultRetailCustomerIdAsync(CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "PERAKENDE-SATIS", cancellationToken);
        return customer?.Id
            ?? throw new InvalidOperationException("\"Perakende Satışlar Carisi\" tanımlı değil. Lütfen sistem yöneticisine başvurun.");
    }

    // Raporlar/Z Listesi ekranından kapanmış (ödemesi alınmış) bir fişi "silmek" için (Edip,
    // 2026-09-03: "restoranda sil mantığı işlesin"). GERÇEK HARD-DELETE YAPILMAZ - kapanış anında
    // CloseCheckAsync'in yazdığı CurrentAccountTransaction/FinancialTransaction/RestaurantPayment
    // satırları MUHASEBE tarafının da kullandığı gerçek cari/kasa hareketleridir (bkz. yorumu),
    // bunları satır satır silmek cari bakiyeyi/kasa mutabakatını sessizce bozardı ve zaten bu
    // kod tabanının HİÇBİR YERİNDE (RestaurantOrderLine.CancelledAtUtc dahil) hard-delete YOK -
    // her yerde ters kayıt (ReversalOfId) deseni var, bkz. [[feedback_sahinsoft_conventions]].
    // Bu yüzden: RetailSale.Status=Cancelled + her hareket için ReversalOfId ile işaretli, ters
    // işaretli bir "IPTAL-" kaydı (InvoicePostingService'in fatura iptalindeki AYNI desen) -
    // kullanıcı için fiş "gitmiş" gibi görünür (tüm raporlarda zaten Status != Cancelled filtresi
    // var), ama muhasebe geçmişi/denetim izi bozulmaz. Masa/adisyon durumuna DOKUNULMAZ - bilinen,
    // henüz çözülmemiş "masa tekrar açılamıyor" hatasına (deferred) bağımlı olunmasın diye.
    public Task CancelRetailSaleAsync(
        int retailSaleId,
        string cancelledByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("İptal gerekçesi zorunludur.");
        }

        return DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var retailSale = await dbContext.RetailSales
                    .Include(x => x.RestaurantCheck).ThenInclude(x => x.Payments)
                    .SingleOrDefaultAsync(x => x.Id == retailSaleId, cancellationToken)
                    ?? throw new InvalidOperationException("Fiş bulunamadı.");

                if (retailSale.Status == RetailSaleStatus.Cancelled)
                {
                    throw new InvalidOperationException("Bu fiş zaten iptal edilmiş.");
                }

                var reversalDocumentNumber = $"IPTAL-{retailSale.DocumentNumber}";

                // GERÇEK EKSİK (2026-09-06, Z dönem kapatma test talimatıyla bulundu) - fiş iptali
                // finansal/cari tarafı ters kayıtla düzeltiyordu ama stok hiç geri eklenmiyordu;
                // iptal edilen bir satışın ürünleri satılmamış sayılmalı. Orijinal StockMovement
                // satırlarının AYNI depo/ürününe, ters işaretli (Quantity) yeni bir hareket
                // yazılır - orijinal hareket asla silinmez/değiştirilmez (aynı ters kayıt ilkesi).
                var originalStockMovements = await dbContext.StockMovements
                    .Where(x => x.DocumentNumber == retailSale.DocumentNumber && x.ReversalOfId == null)
                    .ToListAsync(cancellationToken);
                foreach (var originalMovement in originalStockMovements)
                {
                    dbContext.StockMovements.Add(new StockMovement
                    {
                        MovementDateUtc = DateTime.UtcNow,
                        MovementType = StockMovementType.ReturnIn,
                        Quantity = -originalMovement.Quantity,
                        UnitCost = 0,
                        DocumentNumber = reversalDocumentNumber,
                        ProductId = originalMovement.ProductId,
                        WarehouseId = originalMovement.WarehouseId,
                        RestaurantOrderLineId = originalMovement.RestaurantOrderLineId,
                        ReversalOfId = originalMovement.Id,
                        Description = $"Restoran fişi iptali - {retailSale.DocumentNumber} - {reason}"
                    });
                    var product = await dbContext.Products.SingleAsync(x => x.Id == originalMovement.ProductId, cancellationToken);
                    product.StockQuantity -= originalMovement.Quantity;
                }

                var originalAccountTransactions = await dbContext.CurrentAccountTransactions
                    .Where(x => x.DocumentNumber == retailSale.DocumentNumber && x.ReversalOfId == null)
                    .ToListAsync(cancellationToken);
                foreach (var original in originalAccountTransactions)
                {
                    dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = original.TransactionType == CurrentAccountTransactionType.Sale
                            ? CurrentAccountTransactionType.CreditNote
                            : CurrentAccountTransactionType.DebitNote,
                        DocumentNumber = reversalDocumentNumber,
                        CurrencyCode = original.CurrencyCode,
                        ExchangeRate = original.ExchangeRate,
                        Debit = original.Credit,
                        Credit = original.Debit,
                        CustomerId = original.CustomerId,
                        Description = $"Restoran fişi iptali - {retailSale.DocumentNumber} - {reason}",
                        ReversalOfId = original.Id
                    });
                }

                var originalFinancialTransactions = await dbContext.FinancialTransactions
                    .Where(x => x.DocumentNumber == retailSale.DocumentNumber && x.ReversalOfId == null)
                    .ToListAsync(cancellationToken);
                foreach (var original in originalFinancialTransactions)
                {
                    dbContext.FinancialTransactions.Add(new FinancialTransaction
                    {
                        TransactionDateUtc = DateTime.UtcNow,
                        TransactionType = FinancialTransactionType.Payment,
                        DocumentNumber = reversalDocumentNumber,
                        Amount = original.Amount,
                        ExchangeRate = original.ExchangeRate,
                        Description = $"Restoran fişi iptali - {retailSale.DocumentNumber} - {reason}",
                        FinancialAccountId = original.FinancialAccountId,
                        CustomerId = original.CustomerId,
                        ReversalOfId = original.Id
                    });
                }

                // .ToList() ile kopyalanır - aksi halde Add edilen yeni RestaurantPayment (aynı
                // RestaurantCheckId ile) EF'nin change tracker'ı tarafından bu navigation
                // koleksiyonuna otomatik eklenir ve "Collection was modified" hatası verir.
                foreach (var payment in retailSale.RestaurantCheck.Payments.Where(x => !x.IsReversal).ToList())
                {
                    dbContext.RestaurantPayments.Add(new RestaurantPayment
                    {
                        PaymentMethod = payment.PaymentMethod,
                        Amount = payment.Amount,
                        PaidAtUtc = DateTime.UtcNow,
                        RestaurantCheckId = payment.RestaurantCheckId,
                        FinancialAccountId = payment.FinancialAccountId,
                        IsReversal = true,
                        ReversalOfId = payment.Id
                    });
                }

                retailSale.Status = RetailSaleStatus.Cancelled;
                retailSale.CancelledByUserId = cancelledByUserId;
                retailSale.CancelledAtUtc = DateTime.UtcNow;
                retailSale.CancellationReason = reason;
                retailSale.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);
    }

    // --- Masa taşıma/birleştirme (bkz. CLEAN_ROOM_DEVELOPMENT.md §11 Karar 5) ---
    // Faz 2'de yalnızca güvenli, minimal altyapı vardı (ekran yok, adisyon devretme yok).
    // 2026-08-09: gerçek Masa Satış ekranına bağlanırken MergeTableSessionsAsync tamamlandı -
    // artık kaynak oturumun AÇIK adisyonundaki siparişleri hedef oturumun AÇIK adisyonuna
    // gerçekten taşıyor (RestaurantOrder.RestaurantCheckId güncellenir), kaynak adisyon boş
    // olarak İptal edilir. Henüz ödeme/posting olmadığı için (RetailSale/muhasebe kaydı yok)
    // bu bir "ters kayıt" değil, sadece henüz faturalanmamış siparişlerin taşınması.

    public Task MoveTableSessionAsync(
        int restaurantTableSessionId,
        int toRestaurantTableId,
        string movedByUserId,
        string? reason,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var session = await dbContext.RestaurantTableSessions
                    .SingleOrDefaultAsync(x => x.Id == restaurantTableSessionId, cancellationToken)
                    ?? throw new InvalidOperationException("Masa oturumu bulunamadı.");

                if (session.Status != RestaurantTableSessionStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca açık oturumlar taşınabilir.");
                }

                if (session.RestaurantTableId == toRestaurantTableId)
                {
                    throw new InvalidOperationException("Hedef masa mevcut masayla aynı olamaz.");
                }

                var targetTable = await dbContext.RestaurantTables
                    .SingleOrDefaultAsync(x => x.Id == toRestaurantTableId && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Hedef masa bulunamadı veya pasif.");

                var targetOccupied = await dbContext.RestaurantTableSessions
                    .AnyAsync(x => x.RestaurantTableId == toRestaurantTableId && x.Status == RestaurantTableSessionStatus.Open, cancellationToken);
                if (targetOccupied)
                {
                    throw new InvalidOperationException("Hedef masada zaten açık bir oturum var.");
                }

                dbContext.RestaurantTableSessionMoves.Add(new RestaurantTableSessionMove
                {
                    RestaurantTableSessionId = session.Id,
                    FromRestaurantTableId = session.RestaurantTableId,
                    ToRestaurantTableId = targetTable.Id,
                    MovedAtUtc = DateTime.UtcNow,
                    MovedByUserId = movedByUserId,
                    Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()
                });

                session.RestaurantTableId = targetTable.Id;
                session.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);

    public Task MergeTableSessionsAsync(
        int fromRestaurantTableSessionId,
        int intoRestaurantTableSessionId,
        string mergedByUserId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                if (fromRestaurantTableSessionId == intoRestaurantTableSessionId)
                {
                    throw new InvalidOperationException("Bir oturum kendisiyle birleştirilemez.");
                }

                var from = await dbContext.RestaurantTableSessions
                    .SingleOrDefaultAsync(x => x.Id == fromRestaurantTableSessionId, cancellationToken)
                    ?? throw new InvalidOperationException("Kaynak oturum bulunamadı.");
                var into = await dbContext.RestaurantTableSessions
                    .SingleOrDefaultAsync(x => x.Id == intoRestaurantTableSessionId, cancellationToken)
                    ?? throw new InvalidOperationException("Hedef oturum bulunamadı.");

                if (from.Status != RestaurantTableSessionStatus.Open || into.Status != RestaurantTableSessionStatus.Open)
                {
                    throw new InvalidOperationException("Yalnızca iki açık oturum birleştirilebilir.");
                }

                var fromCheck = await dbContext.RestaurantChecks
                    .SingleOrDefaultAsync(x => x.RestaurantTableSessionId == from.Id && x.Status == RestaurantCheckStatus.Open, cancellationToken);
                var intoCheck = await dbContext.RestaurantChecks
                    .SingleOrDefaultAsync(x => x.RestaurantTableSessionId == into.Id && x.Status == RestaurantCheckStatus.Open, cancellationToken)
                    ?? throw new InvalidOperationException("Hedef masada açık adisyon bulunamadı.");

                if (fromCheck is not null)
                {
                    var ordersToMove = await dbContext.RestaurantOrders
                        .Where(x => x.RestaurantCheckId == fromCheck.Id)
                        .ToListAsync(cancellationToken);
                    foreach (var order in ordersToMove)
                    {
                        order.RestaurantCheckId = intoCheck.Id;
                        order.UpdatedAtUtc = DateTime.UtcNow;
                    }

                    // Taşınan siparişlerin tutarı hedef adisyonun toplamına yansısın diye Check
                    // özet alanları da (SubtotalAmount/TaxAmount/GrandTotal) burada güncellenir -
                    // ClosePayment zaten kapanışta yeniden hesaplıyor ama arada ekranda doğru
                    // görünmesi için şimdiden toplanır.
                    intoCheck.SubtotalAmount += fromCheck.SubtotalAmount;
                    intoCheck.DiscountAmount += fromCheck.DiscountAmount;
                    intoCheck.TaxAmount += fromCheck.TaxAmount;
                    intoCheck.GrandTotal += fromCheck.GrandTotal;
                    intoCheck.UpdatedAtUtc = DateTime.UtcNow;

                    fromCheck.Status = RestaurantCheckStatus.Cancelled;
                    fromCheck.CancelledAtUtc = DateTime.UtcNow;
                    fromCheck.CancelledByUserId = mergedByUserId;
                    fromCheck.CancellationReason = $"Masa birleştirildi (hedef adisyon #{intoCheck.CheckNumber}).";
                    fromCheck.SubtotalAmount = 0;
                    fromCheck.DiscountAmount = 0;
                    fromCheck.TaxAmount = 0;
                    fromCheck.GrandTotal = 0;
                    fromCheck.UpdatedAtUtc = DateTime.UtcNow;
                }

                from.MergedIntoSessionId = into.Id;
                from.Status = RestaurantTableSessionStatus.Closed;
                from.ClosedAtUtc = DateTime.UtcNow;
                from.ClosedByUserId = mergedByUserId;
                from.UpdatedAtUtc = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);

    // Ürün bazlı masa transferi (Edip, 2026-09-28: "ürün bazlı masa transfer yapabileyim, ekranda
    // ürünleri tıkladığımda sarı olsun, seçtiğim ürünleri o masaya transfer etsin, ekran kalan
    // ürünlerle devam etsin") - MoveTableSessionAsync/MergeTableSessionsAsync'in TAMAMINI (bütün
    // adisyonu) taşıdığı yerde, bu SADECE seçilen RestaurantOrderLine'ları taşır. Hedef masa
    // boşsa TransferSelfSaleToTableAsync'teki AYNI "yeni oturum/adisyon aç" deseniyle otomatik
    // açılır; doluysa mevcut açık adisyonuna eklenir. Seçilen satırlar YENİ bir RestaurantOrder
    // altında hedef adisyona bağlanır (RestaurantOrderLine.RestaurantOrderId güncellenir) - ürünün
    // kendisi/KitchenTicketLine geçmişi bozulmaz, sadece hangi adisyona ait olduğu değişir.
    public Task TransferOrderLinesAsync(
        int fromCheckId,
        IReadOnlyCollection<int> orderLineIds,
        int targetTableId,
        string userId,
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                dbContext.ChangeTracker.Clear();
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                if (orderLineIds.Count == 0)
                {
                    throw new InvalidOperationException("Aktarılacak ürün seçilmedi.");
                }

                var fromCheck = await dbContext.RestaurantChecks
                    .Include(x => x.RestaurantTableSession)
                    .SingleOrDefaultAsync(x => x.Id == fromCheckId && x.Status == RestaurantCheckStatus.Open, cancellationToken)
                    ?? throw new InvalidOperationException("Kaynak adisyon bulunamadı ya da açık değil.");

                var targetTable = await dbContext.RestaurantTables
                    .SingleOrDefaultAsync(x => x.Id == targetTableId && x.IsActive, cancellationToken)
                    ?? throw new InvalidOperationException("Hedef masa bulunamadı veya pasif.");

                if (targetTable.Id == fromCheck.RestaurantTableSession.RestaurantTableId)
                {
                    throw new InvalidOperationException("Hedef masa mevcut masayla aynı olamaz.");
                }

                var lines = await dbContext.RestaurantOrderLines
                    .Include(x => x.RestaurantOrder)
                    .Where(x => orderLineIds.Contains(x.Id)
                        && x.RestaurantOrder.RestaurantCheckId == fromCheckId
                        && x.Status != RestaurantOrderLineStatus.Cancelled
                        && x.SettledByPendingPaymentId == null)
                    .ToListAsync(cancellationToken);

                if (lines.Count != orderLineIds.Count)
                {
                    // Ürün bazlı tahsilat (Edip, 2026-09-28: "ödemesi alınan ürünler... bir daha
                    // işlem yaptırmasın") - ön yüz zaten bu satırları tıklanamaz yapıyor, bu sadece
                    // ikinci bir güvenlik katmanı (ör. eski/önbelleğe alınmış bir sayfadan gönderim).
                    throw new InvalidOperationException("Seçilen ürünlerden bazıları bu adisyonda bulunamadı ya da zaten ödemesi alınmış.");
                }

                var remainingLineCount = await dbContext.RestaurantOrderLines
                    .CountAsync(x => x.RestaurantOrder.RestaurantCheckId == fromCheckId
                        && x.Status != RestaurantOrderLineStatus.Cancelled
                        && !orderLineIds.Contains(x.Id), cancellationToken);
                if (remainingLineCount == 0)
                {
                    throw new InvalidOperationException("Adisyondaki TÜM ürünler seçildi - bunun için Masa Taşı'yı kullanın.");
                }

                var targetSession = await dbContext.RestaurantTableSessions
                    .Include(x => x.Checks)
                    .SingleOrDefaultAsync(x => x.RestaurantTableId == targetTableId && x.Status == RestaurantTableSessionStatus.Open, cancellationToken);

                RestaurantCheck targetCheck;
                if (targetSession is not null)
                {
                    targetCheck = targetSession.Checks.SingleOrDefault(x => x.Status == RestaurantCheckStatus.Open)
                        ?? throw new InvalidOperationException("Hedef masada açık adisyon bulunamadı.");
                }
                else
                {
                    // Masa boşsa mevcut masa açma kurallarıyla (OpenTableSessionAsync/
                    // TransferSelfSaleToTableAsync'teki AYNI adımlar) yeni oturum/adisyon açılır.
                    var newSession = new RestaurantTableSession
                    {
                        RestaurantTableId = targetTable.Id,
                        Status = RestaurantTableSessionStatus.Open,
                        OpenedAtUtc = DateTime.UtcNow,
                        OpenedByUserId = userId,
                        GuestCount = 1
                    };
                    dbContext.RestaurantTableSessions.Add(newSession);

                    var newCheckNumber = await documentNumberGenerator.GenerateWithinTransactionAsync("RESTAURANT_CHECK", cancellationToken);
                    var newCheck = new RestaurantCheck
                    {
                        CheckNumber = newCheckNumber,
                        Status = RestaurantCheckStatus.Open,
                        OpenedAtUtc = DateTime.UtcNow,
                        RestaurantTableSession = newSession
                    };
                    dbContext.RestaurantChecks.Add(newCheck);
                    await dbContext.SaveChangesAsync(cancellationToken);
                    targetCheck = newCheck;
                }

                var transferOrder = new RestaurantOrder
                {
                    OrderedAtUtc = DateTime.UtcNow,
                    OrderedByUserId = userId,
                    RestaurantCheckId = targetCheck.Id
                };
                dbContext.RestaurantOrders.Add(transferOrder);
                await dbContext.SaveChangesAsync(cancellationToken);

                foreach (var line in lines)
                {
                    line.RestaurantOrderId = transferOrder.Id;
                    line.UpdatedAtUtc = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;
            });
        }, cancellationToken);
}
