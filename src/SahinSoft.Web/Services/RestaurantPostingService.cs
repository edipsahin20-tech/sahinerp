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
    DocumentNumberGeneratorService documentNumberGenerator)
{
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
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

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
        CancellationToken cancellationToken = default) =>
        DocumentNumberGeneratorService.ExecuteWithConcurrencyRetryAsync(dbContext, () =>
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var existingByKey = await dbContext.PackageOrders
                    .SingleOrDefaultAsync(x => x.SubmissionKey == submissionKey, cancellationToken);
                if (existingByKey is not null)
                {
                    return existingByKey;
                }

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
                    Status = PackageOrderStatus.Preparing,
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
            .Include(x => x.RestaurantTableSession)
            .SingleOrDefaultAsync(x => x.Id == restaurantCheckId, cancellationToken)
            ?? throw new InvalidOperationException("Adisyon bulunamadı.");

        if (check.Status != RestaurantCheckStatus.Open)
        {
            throw new InvalidOperationException("Yalnızca açık adisyonlara sipariş eklenebilir.");
        }

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

        // KDS takibi kapalıysa (Edip, 2026-09-03: varsayılan kapalı) hiçbir KitchenTicket
        // oluşturulmaz - satırlar doğrudan Servis Edildi sayılır, Mutfak ekranında Hazır/Servis
        // Edildi tıklamalarına gerek kalmaz. Açıksa bugünkü davranış (istasyona göre fiş) aynen
        // sürer.
        var isKitchenTrackingEnabled = await dbContext.InventorySettings
            .Where(x => x.Id == 1)
            .Select(x => x.IsKitchenTrackingEnabled)
            .SingleOrDefaultAsync(cancellationToken);

        if (!isKitchenTrackingEnabled)
        {
            foreach (var line in orderLines)
            {
                line.Status = RestaurantOrderLineStatus.Served;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new SendOrderToKitchenResult(order, []);
        }

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
                continue;
            }

            if (!linesByStation.TryGetValue(stationId.Value, out var group))
            {
                group = [];
                linesByStation[stationId.Value] = group;
            }
            group.Add(line);
        }

        foreach (var (stationId, groupLines) in linesByStation)
        {
            var station = await dbContext.KitchenStations
                .SingleAsync(x => x.Id == stationId, cancellationToken);

            var ticket = new KitchenTicket
            {
                RestaurantOrder = order,
                KitchenStationId = stationId,
                Status = KitchenTicketStatus.Sent,
                SentAtUtc = DateTime.UtcNow
            };
            dbContext.KitchenTickets.Add(ticket);

            foreach (var line in groupLines)
            {
                dbContext.KitchenTicketLines.Add(new KitchenTicketLine
                {
                    KitchenTicket = ticket,
                    RestaurantOrderLine = line,
                    Status = KitchenTicketLineStatus.Sent
                });
                // Fiş gönderildiği an satırın hazırlık durumu Preparing'e geçer — kalan durum geçişleri
                // (InProgress/Ready/Served) Faz 3'teki mutfak ekranından KitchenTicketLine üzerinden
                // yapılacak, RestaurantOrderLine.Status o zaman ticket satırlarından yeniden hesaplanır.
                line.Status = RestaurantOrderLineStatus.Preparing;
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
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new SendOrderToKitchenResult(order, unroutedProductNames);
    }

    public Task CancelOrderLineAsync(
        int restaurantOrderLineId,
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
                return true;
            });
        }, cancellationToken);
    }

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
                var cashInDuringShift = await dbContext.RestaurantPayments
                    .Where(x => x.FinancialAccountId == shift.FinancialAccountId
                        && !x.IsReversal
                        && x.PaidAtUtc >= shift.OpenedAtUtc
                        && x.PaidAtUtc <= closedAt)
                    .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

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
                await using var transaction = await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                var check = await dbContext.RestaurantChecks
                    .Include(x => x.Orders).ThenInclude(x => x.Lines)
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

                var lines = check.Orders
                    .SelectMany(o => o.Lines)
                    .Where(l => l.Status != RestaurantOrderLineStatus.Cancelled)
                    .ToList();
                if (lines.Count == 0)
                {
                    throw new InvalidOperationException("Boş adisyon kapatılamaz.");
                }

                if (payments.Count == 0)
                {
                    throw new InvalidOperationException("En az bir ödeme satırı girilmelidir.");
                }

                decimal subtotal = 0, tax = 0, discount = 0, grandTotal = 0;
                var retailSaleLines = new List<RetailSaleLine>();
                foreach (var line in lines)
                {
                    var lineTotal = Math.Round(line.Quantity * line.UnitPriceSnapshot - line.DiscountAmountSnapshot, 2, MidpointRounding.AwayFromZero);
                    var (matrah, kdvTutari) = RestaurantPricingCalculator.ExtractTax(lineTotal, line.TaxRateSnapshot);
                    subtotal += matrah;
                    tax += kdvTutari;
                    discount += line.DiscountAmountSnapshot;
                    grandTotal += lineTotal;

                    retailSaleLines.Add(new RetailSaleLine
                    {
                        ProductNameSnapshot = line.ProductNameSnapshot,
                        Quantity = line.Quantity,
                        UnitPriceSnapshot = line.UnitPriceSnapshot,
                        TaxRateSnapshot = line.TaxRateSnapshot,
                        DiscountAmountSnapshot = line.DiscountAmountSnapshot,
                        LineTotal = lineTotal,
                        ProductId = line.ProductId
                    });
                }

                grandTotal = Math.Round(grandTotal, 2, MidpointRounding.AwayFromZero);
                var paymentsTotal = Math.Round(payments.Sum(p => p.Amount), 2, MidpointRounding.AwayFromZero);
                if (paymentsTotal != grandTotal)
                {
                    throw new InvalidOperationException($"Ödeme toplamı ({paymentsTotal:N2}) adisyon tutarına ({grandTotal:N2}) eşit değil.");
                }

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

                dbContext.CurrentAccountTransactions.Add(new CurrentAccountTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = CurrentAccountTransactionType.Sale,
                    DocumentNumber = documentNumber,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    Debit = grandTotal,
                    Credit = 0,
                    CustomerId = effectiveCustomerId,
                    Description = $"Restoran satışı - {check.CheckNumber}"
                });

                var collectionAccountTransaction = new CurrentAccountTransaction
                {
                    TransactionDateUtc = DateTime.UtcNow,
                    TransactionType = CurrentAccountTransactionType.Collection,
                    DocumentNumber = documentNumber,
                    CurrencyCode = "TRY",
                    ExchangeRate = 1,
                    Debit = 0,
                    Credit = paymentsTotal,
                    CustomerId = effectiveCustomerId,
                    Description = $"Restoran tahsilatı - {check.CheckNumber}"
                };
                dbContext.CurrentAccountTransactions.Add(collectionAccountTransaction);

                foreach (var payment in payments)
                {
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
                        CheckNumber = check.CheckNumber
                    })
                });

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return retailSale;
            });
        }, cancellationToken);

    private async Task<int> GetDefaultRetailCustomerIdAsync(CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == "PERAKENDE-SATIS", cancellationToken);
        return customer?.Id
            ?? throw new InvalidOperationException("\"Perakende Satışlar Carisi\" tanımlı değil. Lütfen sistem yöneticisine başvurun.");
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
}
