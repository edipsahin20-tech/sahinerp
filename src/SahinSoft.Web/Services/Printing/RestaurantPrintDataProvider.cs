using Microsoft.EntityFrameworkCore;
using SahinSoft.Domain.Entities;
using SahinSoft.Domain.Enums;
using SahinSoft.Web.Data;

namespace SahinSoft.Web.Services.Printing;

// Çıktı Tasarımcısı'nın gerçek veri kaynağı - her TemplateType için gerçek tablolardan
// (RetailSale/KitchenTicket/RestaurantZPeriod/RestaurantPayment/RestaurantOrderLine) okur, Test
// Yazdır için sabit örnek veri döner. Restoran raporlama controller'larının kendi view-model
// mantığını TEKRARLAMAZ - bağımsız, sadece çıktı için gereken alanları hesaplar.
public sealed class RestaurantPrintDataProvider(ApplicationDbContext dbContext) : IPrintDataProvider
{
    public static string PaymentMethodLabel(RestaurantPaymentMethod method) => method switch
    {
        RestaurantPaymentMethod.Cash => "Nakit",
        RestaurantPaymentMethod.CreditCard => "Kredi Kartı",
        RestaurantPaymentMethod.MealCard => "Yemek Kartı",
        RestaurantPaymentMethod.Unpaid => "Ödenmez",
        RestaurantPaymentMethod.OpenAccount => "Açık Hesap",
        _ => method.ToString()
    };

    public async Task<PrintDataContext> BuildForRetailSaleAsync(int retailSaleId, CancellationToken cancellationToken = default)
    {
        var sale = await dbContext.RetailSales
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Lines)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable)
            .Include(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x.Branch)
            .SingleAsync(x => x.Id == retailSaleId, cancellationToken);

        var payments = await dbContext.RestaurantPayments
            .AsNoTracking()
            .Where(x => x.RestaurantCheckId == sale.RestaurantCheckId && !x.IsReversal)
            .ToListAsync(cancellationToken);

        var branch = sale.RestaurantCheck.RestaurantTableSession.Branch;
        var company = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1, cancellationToken);
        var cashierUserId = sale.RestaurantCheck.RestaurantTableSession.OpenedByUserId;
        var cashierName = await dbContext.Users.AsNoTracking().Where(x => x.Id == cashierUserId).Select(x => x.FullName).SingleOrDefaultAsync(cancellationToken) ?? cashierUserId;

        var channel = sale.RestaurantCheck.RestaurantTableSession.Channel;
        var saleType = channel switch
        {
            RestaurantSaleChannel.SelfSatis => "Self Satış",
            RestaurantSaleChannel.Paket => "Paket",
            _ => "Masa"
        };

        // GERÇEK HATA düzeltmesi (Edip, 2026-09-29: "Self/Paket'te masa alanı yoksa sahte masa
        // adı basmamalı, satış tipi/fiş no gösterilmeli") - önceden bu ikisi HER ZAMAN gerçek bir
        // RestaurantTable/RestaurantSection'a bağlıydı (Self/Paket için de sahte, tek kullanımlık
        // bir masa/salon üretiliyordu). Artık masasız kanallarda gerçek bir masa/bölüm YOK -
        // Masa'da gerçek isimler, Self/Paket'te satış tipi/fiş no gösterilir.
        var tableNameForPrint = channel == RestaurantSaleChannel.Masa
            ? sale.RestaurantCheck.RestaurantTableSession.RestaurantTable?.Name ?? ""
            : channel == RestaurantSaleChannel.Paket
                ? (await dbContext.PackageOrders.AsNoTracking().Where(x => x.RestaurantCheckId == sale.RestaurantCheckId).Select(x => x.PackageNumber).SingleOrDefaultAsync(cancellationToken)) ?? sale.DocumentNumber
                : sale.DocumentNumber;
        var sectionNameForPrint = saleType;

        var ctx = new PrintDataContext();
        FillCommonHeader(ctx, company, branch.Name, branch.Address, branch.Phone);
        ctx.Texts["adisyon.checkNumber"] = sale.DocumentNumber;
        ctx.Texts["common.dateTime"] = sale.IssuedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        ctx.Texts["adisyon.tableName"] = tableNameForPrint;
        ctx.Texts["adisyon.sectionName"] = sectionNameForPrint;
        ctx.Texts["adisyon.guestCount"] = sale.RestaurantCheck.RestaurantTableSession.GuestCount.ToString();
        ctx.Texts["adisyon.cashierName"] = cashierName;
        ctx.Texts["adisyon.saleType"] = saleType;
        ctx.Texts["adisyon.note"] = sale.RestaurantCheck.Note ?? "";
        // GERÇEK HATA (2026-09-28, canlı test yazdırmada bulundu): "ARA TOPLAM" önceden
        // sale.SubtotalAmount'a (KDV'den ARINDIRILMIŞ net matrah) bağlıydı - müşteri fişinde bu,
        // GENEL TOPLAM'dan (KDV DAHİL) farklı bir tabanda göründüğü için indirim 0 iken bile
        // ARA TOPLAM ≠ GENEL TOPLAM gibi yanlış/tutarsız bir görüntü veriyordu. Doğrusu: ARA
        // TOPLAM = GrandTotal + DiscountAmount (KDV DAHİL, indirim UYGULANMADAN ÖNCEKİ ürün
        // toplamı) - ARA TOPLAM - İNDİRİM = GENEL TOPLAM eşitliği artık her zaman sağlanır.
        // sale.SubtotalAmount (KDV hariç matrah) hâlâ ayrı "adisyon.tax" ile birlikte X/Z gibi
        // vergi dökümü gereken yerler için kullanılabilir, sadece varsayılan Adisyon şablonunda
        // KULLANILMIYOR (referans görselde de adisyon fişinde ayrıştırılmış KDV satırı yok).
        ctx.Texts["adisyon.subtotal"] = (sale.GrandTotal + sale.DiscountAmount).ToString("N2") + " ₺";
        ctx.Texts["adisyon.discount"] = sale.DiscountAmount.ToString("N2") + " ₺";
        ctx.Texts["adisyon.tax"] = sale.TaxAmount.ToString("N2") + " ₺";
        ctx.Texts["adisyon.grandTotal"] = sale.GrandTotal.ToString("N2") + " ₺";
        var paidTotal = payments.Sum(p => p.Amount);
        ctx.Texts["adisyon.changeAmount"] = Math.Max(paidTotal - sale.GrandTotal, 0).ToString("N2") + " ₺";
        ctx.Codes["common.qrCode"] = sale.DocumentNumber;
        ctx.Codes["common.barcode"] = sale.DocumentNumber;

        ctx.LineItems = sale.Lines.Select(l => new PrintLineItem(l.Quantity, l.ProductNameSnapshot, l.UnitPriceSnapshot, l.LineTotal)).ToList();
        ctx.PaymentBreakdown = payments
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PrintBreakdownRow(PaymentMethodLabel(g.Key), g.Count(), g.Sum(p => p.Amount)))
            .ToList();

        return ctx;
    }

    public async Task<PrintDataContext> BuildForKitchenTicketAsync(int kitchenTicketId, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.KitchenTickets
            .AsNoTracking()
            .Include(x => x.Lines).ThenInclude(x => x.RestaurantOrderLine)
            .Include(x => x.RestaurantOrder).ThenInclude(x => x.RestaurantCheck).ThenInclude(x => x.RestaurantTableSession).ThenInclude(x => x!.RestaurantTable).ThenInclude(x => x.RestaurantSection)
            .SingleAsync(x => x.Id == kitchenTicketId, cancellationToken);

        var check = ticket.RestaurantOrder.RestaurantCheck;
        var cashierUserId = check.RestaurantTableSession.OpenedByUserId;
        var cashierName = await dbContext.Users.AsNoTracking().Where(x => x.Id == cashierUserId).Select(x => x.FullName).SingleOrDefaultAsync(cancellationToken) ?? cashierUserId;

        // Mutfak fişi Paket siparişlerinde de basılabilir (sadece Self Satış mutfağa hiç
        // gitmiyor) - masasız kanallarda sahte masa adı basılmaz, satış tipi/fiş no gösterilir.
        var ticketChannel = check.RestaurantTableSession.Channel;
        var mutfakTableName = ticketChannel == RestaurantSaleChannel.Masa
            ? check.RestaurantTableSession.RestaurantTable?.Name ?? ""
            : ticketChannel == RestaurantSaleChannel.Paket
                ? (await dbContext.PackageOrders.AsNoTracking().Where(x => x.RestaurantCheckId == check.Id).Select(x => x.PackageNumber).SingleOrDefaultAsync(cancellationToken)) ?? check.CheckNumber
                : check.CheckNumber;
        var mutfakSectionName = ticketChannel switch
        {
            RestaurantSaleChannel.Paket => "Paket",
            RestaurantSaleChannel.SelfSatis => "Self Satış",
            _ => check.RestaurantTableSession.RestaurantTable?.RestaurantSection.Name ?? ""
        };

        var ctx = new PrintDataContext();
        ctx.Texts["mutfak.tableName"] = mutfakTableName;
        ctx.Texts["mutfak.sectionName"] = mutfakSectionName;
        ctx.Texts["mutfak.orderNumber"] = ticket.TicketNumber ?? ("#" + ticket.Id);
        ctx.Texts["mutfak.dateTime"] = ticket.SentAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        ctx.Texts["mutfak.cashierName"] = cashierName;
        ctx.Texts["common.dateTime"] = ticket.SentAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        ctx.LineItems = ticket.Lines.Select(l => new PrintLineItem(
            l.RestaurantOrderLine.Quantity,
            l.RestaurantOrderLine.ProductNameSnapshot,
            0,
            null)).ToList();

        return ctx;
    }

    public async Task<PrintDataContext> BuildForZPeriodAsync(int zPeriodId, CancellationToken cancellationToken = default)
    {
        var period = await dbContext.RestaurantZPeriods.AsNoTracking().SingleAsync(x => x.Id == zPeriodId, cancellationToken);
        var branch = await dbContext.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == period.BranchId, cancellationToken);
        var company = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1, cancellationToken);
        var closedByName = period.ClosedAutomatically || period.ClosedByUserId is null
            ? "Otomatik"
            : (await dbContext.Users.AsNoTracking().Where(x => x.Id == period.ClosedByUserId).Select(x => x.FullName).SingleOrDefaultAsync(cancellationToken)) ?? period.ClosedByUserId;

        var ctx = new PrintDataContext();
        FillCommonHeader(ctx, company, branch?.Name ?? "", branch?.Address, branch?.Phone);
        ctx.Texts["report.title"] = "Z RAPORU";
        ctx.Texts["report.zNo"] = "#" + period.Id;
        ctx.Texts["report.reportDate"] = (period.ClosedAtUtc ?? period.OpenedAtUtc).ToLocalTime().ToString("dd.MM.yyyy");
        ctx.Texts["report.reportTime"] = (period.ClosedAtUtc ?? period.OpenedAtUtc).ToLocalTime().ToString("HH:mm");
        ctx.Texts["common.dateTime"] = (period.ClosedAtUtc ?? period.OpenedAtUtc).ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        ctx.Texts["report.openedAt"] = period.OpenedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        ctx.Texts["report.closedAt"] = period.ClosedAtUtc?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "-";
        ctx.Texts["report.cashierName"] = closedByName;
        ctx.Texts["report.receiptCount"] = period.ReceiptCount.ToString();
        ctx.Texts["report.grossTotal"] = period.GrossTotal.ToString("N2") + " ₺";
        ctx.Texts["report.discountTotal"] = period.DiscountTotal.ToString("N2") + " ₺";
        ctx.Texts["report.netTotal"] = period.NetTotal.ToString("N2") + " ₺";
        ctx.Texts["report.totalSalesRow"] = period.ReceiptCount + " fiş / " + period.NetTotal.ToString("N2") + " ₺";

        await FillBreakdownsAsync(ctx, saleIds: await dbContext.RetailSales.AsNoTracking().Where(x => x.RestaurantZPeriodId == zPeriodId).Select(x => x.Id).ToListAsync(cancellationToken), cancellationToken);

        return ctx;
    }

    public async Task<PrintDataContext> BuildForXReportAsync(int branchId, CancellationToken cancellationToken = default)
    {
        // X Raporu = ara rapor, kapanmamış AKTİF Z döneminin o ana kadarki verisi (dondurulmuş
        // değil, canlı hesaplanır).
        var period = await dbContext.RestaurantZPeriods
            .AsNoTracking()
            .Where(x => x.BranchId == branchId && x.Status == RestaurantZPeriodStatus.Open)
            .SingleOrDefaultAsync(cancellationToken);

        var branch = await dbContext.Branches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == branchId, cancellationToken);
        var company = await dbContext.CompanySettings.AsNoTracking().SingleAsync(x => x.Id == 1, cancellationToken);

        var saleIds = period is null
            ? []
            : await dbContext.RetailSales.AsNoTracking().Where(x => x.RestaurantZPeriodId == period.Id).Select(x => x.Id).ToListAsync(cancellationToken);
        var sales = await dbContext.RetailSales.AsNoTracking().Where(x => saleIds.Contains(x.Id)).ToListAsync(cancellationToken);
        var activeSales = sales.Where(x => x.Status != RetailSaleStatus.Cancelled).ToList();

        var ctx = new PrintDataContext();
        FillCommonHeader(ctx, company, branch?.Name ?? "", branch?.Address, branch?.Phone);
        ctx.Texts["report.title"] = "X RAPORU (Günlük Ara Rapor)";
        ctx.Texts["report.reportDate"] = DateTime.Now.ToString("dd.MM.yyyy");
        ctx.Texts["report.reportTime"] = DateTime.Now.ToString("HH:mm");
        ctx.Texts["common.dateTime"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        ctx.Texts["report.openedAt"] = period?.OpenedAtUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "-";
        ctx.Texts["report.receiptCount"] = activeSales.Count.ToString();

        // GERÇEK HATA (2026-09-28, Edip: "X,Z raporlarında ödeme tiplerinde herşey KDV dahil
        // gözüksün, ekstra KDV hesabı yapma, herşey net olacak") - burada TOPLAM SATIŞ daha önce
        // activeSales.Sum(GrandTotal) idi, yani ÖDENMEZ tutarını da ciroya dahil ediyordu; web
        // tarafındaki AYNI rapor (RestaurantReportsController.XReport.NetRevenue) zaten doğru
        // şekilde SADECE gerçek tahsil edilen ödeme yöntemlerini (Nakit+Kredi Kartı+Açık Hesap+
        // Yemek Çeki - Ödenmez HARİÇ) topluyordu - ikisi arasında fark vardı. Z Raporu'nun
        // (RestaurantPostingService.CloseActiveZPeriodAsync) NetTotal'ı zaten bu şekilde doğru
        // hesaplanıyor, X Raporu'nu da AYNI mantığa (Ödenmez hariç ödeme toplamı) getiriyoruz -
        // KDV hiçbir yerde ayrıca eklenmiyor/çıkarılmıyor, GrandTotal zaten KDV dahil.
        var checkIds = activeSales.Select(x => x.RestaurantCheckId).ToList();
        var netTotal = checkIds.Count == 0
            ? 0m
            : await dbContext.RestaurantPayments
                .AsNoTracking()
                .Where(p => !p.IsReversal && checkIds.Contains(p.RestaurantCheckId) && p.PaymentMethod != RestaurantPaymentMethod.Unpaid)
                .SumAsync(p => p.Amount, cancellationToken);
        ctx.Texts["report.grossTotal"] = netTotal.ToString("N2") + " ₺";
        ctx.Texts["report.totalSalesRow"] = activeSales.Count + " fiş / " + netTotal.ToString("N2") + " ₺";

        await FillBreakdownsAsync(ctx, saleIds, cancellationToken);

        return ctx;
    }

    private async Task FillBreakdownsAsync(PrintDataContext ctx, List<int> saleIds, CancellationToken cancellationToken)
    {
        var sales = await dbContext.RetailSales.AsNoTracking().Include(x => x.Lines).Where(x => saleIds.Contains(x.Id)).ToListAsync(cancellationToken);
        var activeSales = sales.Where(x => x.Status != RetailSaleStatus.Cancelled).ToList();
        var cancelledSales = sales.Where(x => x.Status == RetailSaleStatus.Cancelled).ToList();
        var checkIds = activeSales.Select(x => x.RestaurantCheckId).ToList();

        var payments = await dbContext.RestaurantPayments.AsNoTracking().Where(p => !p.IsReversal && checkIds.Contains(p.RestaurantCheckId)).ToListAsync(cancellationToken);
        ctx.PaymentBreakdown = payments.GroupBy(p => p.PaymentMethod)
            .Select(g => new PrintBreakdownRow(PaymentMethodLabel(g.Key), g.Count(), g.Sum(p => p.Amount)))
            .ToList();

        var allLines = activeSales.SelectMany(s => s.Lines).ToList();
        var discountLines = allLines.Where(l => !l.IsComplimentary && l.DiscountAmountSnapshot > 0).ToList();
        var complimentaryLines = allLines.Where(l => l.IsComplimentary).ToList();
        var unpaidTotal = payments.Where(p => p.PaymentMethod == RestaurantPaymentMethod.Unpaid).Sum(p => p.Amount);

        ctx.DiscountBreakdown =
        [
            new("İndirim", discountLines.Count, discountLines.Sum(l => l.DiscountAmountSnapshot)),
            new("İkram", complimentaryLines.Count, complimentaryLines.Sum(l => l.DiscountAmountSnapshot)),
            new("Ödenmez", payments.Count(p => p.PaymentMethod == RestaurantPaymentMethod.Unpaid), unpaidTotal)
        ];
        ctx.Texts["report.discountTotal"] = discountLines.Sum(l => l.DiscountAmountSnapshot).ToString("N2") + " ₺";
        ctx.Texts["report.complimentaryTotal"] = complimentaryLines.Sum(l => l.DiscountAmountSnapshot).ToString("N2") + " ₺";
        ctx.Texts["report.unpaidTotal"] = unpaidTotal.ToString("N2") + " ₺";

        var checkIdsForCancel = activeSales.Select(x => x.RestaurantCheckId).Concat(cancelledSales.Select(x => x.RestaurantCheckId)).Distinct().ToList();
        var cancelledLines = await dbContext.RestaurantOrderLines
            .AsNoTracking()
            .Where(x => x.CancelledAtUtc != null && checkIdsForCancel.Contains(x.RestaurantOrder.RestaurantCheckId))
            .ToListAsync(cancellationToken);
        var lineCancelTotal = cancelledLines.Sum(x => x.Quantity * x.UnitPriceSnapshot);
        var receiptCancelTotal = cancelledSales.Sum(x => x.GrandTotal);

        ctx.CancellationBreakdown =
        [
            new("Satır İptal", cancelledLines.Count, lineCancelTotal),
            new("Fiş İptal", cancelledSales.Count, receiptCancelTotal)
        ];
        ctx.Texts["report.lineCancellationTotal"] = lineCancelTotal.ToString("N2") + " ₺";
        ctx.Texts["report.receiptCancellationTotal"] = receiptCancelTotal.ToString("N2") + " ₺";
    }

    private static void FillCommonHeader(PrintDataContext ctx, CompanySettings company, string branchName, string? address, string? phone)
    {
        ctx.Texts["common.companyName"] = company.CompanyName;
        ctx.Texts["common.branchName"] = branchName;
        ctx.Texts["common.address"] = address ?? "";
        ctx.Texts["common.phone"] = phone ?? "";
        ctx.Texts["common.taxOffice"] = (company.TaxOffice ?? "") + (string.IsNullOrEmpty(company.TaxNumber) ? "" : " / " + company.TaxNumber);
        ctx.Texts["common.website"] = company.Website ?? "";
        if (!string.IsNullOrWhiteSpace(company.LogoPath))
        {
            ctx.ImagePaths["common.logo"] = company.LogoPath;
        }
    }

    public PrintDataContext BuildSample(PrintTemplateType type)
    {
        var ctx = new PrintDataContext();
        ctx.Texts["common.companyName"] = "ŞahinSoft Restoran Çözümleri";
        ctx.Texts["common.branchName"] = "Merkez Şube";
        ctx.Texts["common.address"] = "Atatürk Cad. No:123 Kadıköy / İSTANBUL";
        ctx.Texts["common.phone"] = "0216 123 45 67";
        ctx.Texts["common.taxOffice"] = "Ulus V.D. / 1234567890";
        ctx.Texts["common.website"] = "www.sahinbilisim.com.tr";
        ctx.Texts["common.dateTime"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        ctx.Codes["common.qrCode"] = "TEST-0001";
        ctx.Codes["common.barcode"] = "TEST-0001";

        switch (type)
        {
            case PrintTemplateType.Adisyon:
                ctx.Texts["adisyon.checkNumber"] = "AD.00001";
                ctx.Texts["adisyon.tableName"] = "TERAS-4";
                ctx.Texts["adisyon.sectionName"] = "A-TERAS";
                ctx.Texts["adisyon.guestCount"] = "2";
                ctx.Texts["adisyon.cashierName"] = "Test Kasiyer";
                ctx.Texts["adisyon.saleType"] = "Masa";
                ctx.Texts["adisyon.note"] = "";
                ctx.Texts["adisyon.subtotal"] = "700,00 ₺";
                ctx.Texts["adisyon.discount"] = "0,00 ₺";
                ctx.Texts["adisyon.tax"] = "63,64 ₺";
                ctx.Texts["adisyon.grandTotal"] = "700,00 ₺";
                ctx.Texts["adisyon.changeAmount"] = "0,00 ₺";
                ctx.LineItems =
                [
                    new(2, "Mercimek Çorbası", 90, 180),
                    new(1, "Adana Kebap", 320, 320),
                    new(2, "Ayran", 40, 80),
                    new(1, "Fındık Lahmacun", 120, 120)
                ];
                ctx.PaymentBreakdown = [new("Nakit", 1, 700)];
                break;
            case PrintTemplateType.MutfakFisi:
                ctx.Texts["mutfak.tableName"] = "TERAS-4";
                ctx.Texts["mutfak.sectionName"] = "A-TERAS";
                ctx.Texts["mutfak.orderNumber"] = "#32247";
                ctx.Texts["mutfak.cashierName"] = "Test Kasiyer";
                ctx.LineItems = [new(1, "Menemen", 0, null)];
                break;
            case PrintTemplateType.XRaporu:
            case PrintTemplateType.ZRaporu:
                ctx.Texts["report.title"] = type == PrintTemplateType.ZRaporu ? "Z RAPORU (Gün Sonu Raporu)" : "X RAPORU (Günlük Ara Rapor)";
                ctx.Texts["report.zNo"] = "Z-000020";
                ctx.Texts["report.reportDate"] = DateTime.Now.ToString("dd.MM.yyyy");
                ctx.Texts["report.reportTime"] = DateTime.Now.ToString("HH:mm");
                ctx.Texts["report.openedAt"] = DateTime.Now.AddHours(-8).ToString("dd.MM.yyyy HH:mm");
                ctx.Texts["report.closedAt"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                ctx.Texts["report.cashierName"] = "Test Kasiyer";
                ctx.Texts["report.receiptCount"] = "150";
                ctx.Texts["report.grossTotal"] = "54.805,00 ₺";
                ctx.Texts["report.discountTotal"] = "2.140,50 ₺";
                ctx.Texts["report.complimentaryTotal"] = "1.680,00 ₺";
                ctx.Texts["report.unpaidTotal"] = "12.175,00 ₺";
                ctx.Texts["report.netTotal"] = "36.919,50 ₺";
                ctx.Texts["report.totalSalesRow"] = "150 fiş / 54.805,00 ₺";
                ctx.PaymentBreakdown =
                [
                    new("Nakit", 30, 11521.41m),
                    new("Kredi Kartı", 30, 12691.11m),
                    new("Açık Hesap", 30, 8610.00m),
                    new("Ödenmez", 30, 12175.00m),
                    new("Fiş İkram", 30, 9730.00m)
                ];
                ctx.DiscountBreakdown =
                [
                    new("İndirim", 20, 2140.50m),
                    new("İkram", 12, 1680.00m),
                    new("Ödenmez", 30, 12175.00m)
                ];
                ctx.CancellationBreakdown =
                [
                    new("Satır İptal", 8, 650.00m),
                    new("Fiş İptal", 4, 1240.00m)
                ];
                break;
        }

        return ctx;
    }
}
