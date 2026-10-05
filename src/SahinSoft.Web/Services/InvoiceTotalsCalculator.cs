using SahinSoft.Domain.Entities;

namespace SahinSoft.Web.Services;

// Satır ve fatura toplamlarını (Subtotal/DiscountTotal/TaxTotal/GrandTotal) hesaplar; hem taslak
// kaydında (InvoicesController) hem onay anında (InvoicePostingService) aynı formülün kullanılması
// için tek yerde tutulur. Genel Tutar İskontosu, KDV matrahını doğru düşürmek için satırlar arasında
// net tutar payına göre orantılı dağıtılır (Teklif Stüdyosu'ndaki mantıkla birebir aynı).
public static class InvoiceTotalsCalculator
{
    public static void Calculate(Invoice invoice)
    {
        // "Fiyatlara KDV Dahil" ile girilmiş (TL) faturalar KDV DAHİL hesaplanır: satır toplamı = miktar × girilen fiyat (2 hane);
        // KDV hariç matrah ve KDV bundan türetilir, böylece ekranda girilen = faturadaki = cari tutarı birebir aynı olur.
        if (invoice.Lines.Count > 0 && invoice.Lines.All(x => x.UnitPriceInclTax is not null))
        {
            CalculateInclusive(invoice);
            return;
        }

        var lines = invoice.Lines.OrderBy(x => x.LineNumber).ToList();

        var lineCalc = lines.Select(line =>
        {
            var gross = RoundMoney(line.Quantity * line.UnitPrice);
            var lineDiscAmount = RoundMoney(gross * line.DiscountRate / 100);
            var netAfterLineDiscount = gross - lineDiscAmount;
            return (line, gross, lineDiscAmount, netAfterLineDiscount);
        }).ToList();

        var netAfterLineDiscountTotal = lineCalc.Sum(x => x.netAfterLineDiscount);
        var amountDiscount = Math.Clamp(invoice.AmountDiscount, 0, Math.Max(netAfterLineDiscountTotal, 0));

        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0, allocatedAmountDiscount = 0;
        for (var i = 0; i < lineCalc.Count; i++)
        {
            var (line, gross, lineDiscAmount, netAfterLineDiscount) = lineCalc[i];

            decimal amountDiscShare;
            if (i == lineCalc.Count - 1)
            {
                amountDiscShare = RoundMoney(amountDiscount - allocatedAmountDiscount);
            }
            else
            {
                var ratio = netAfterLineDiscountTotal > 0 ? netAfterLineDiscount / netAfterLineDiscountTotal : 0;
                amountDiscShare = RoundMoney(amountDiscount * ratio);
            }
            allocatedAmountDiscount += amountDiscShare;

            line.DiscountAmount = lineDiscAmount + amountDiscShare;
            var net = gross - line.DiscountAmount;
            line.TaxAmount = RoundMoney(net * line.TaxRate / 100);
            line.LineTotal = net + line.TaxAmount;

            subtotal += gross;
            discountTotal += line.DiscountAmount;
            taxTotal += line.TaxAmount;
            grandTotal += line.LineTotal;
        }

        invoice.AmountDiscount = amountDiscount;
        invoice.Subtotal = RoundMoney(subtotal);
        invoice.DiscountTotal = RoundMoney(discountTotal);
        invoice.TaxTotal = RoundMoney(taxTotal);
        invoice.GrandTotal = RoundMoney(grandTotal);
    }

    // KDV dahil hat: tüm indirimler KDV dahil tutar üzerinden uygulanır (genel tutar iskontosu da KDV dahil girilir).
    private static void CalculateInclusive(Invoice invoice)
    {
        var lines = invoice.Lines.OrderBy(x => x.LineNumber).ToList();
        var lineCalc = lines.Select(line =>
        {
            var grossIncl = RoundMoney(line.Quantity * line.UnitPriceInclTax!.Value);
            var lineDiscIncl = RoundMoney(grossIncl * line.DiscountRate / 100);
            return (line, grossIncl, lineDiscIncl, netIncl: grossIncl - lineDiscIncl);
        }).ToList();

        var netInclTotal = lineCalc.Sum(x => x.netIncl);
        var amountDiscount = Math.Clamp(invoice.AmountDiscount, 0, Math.Max(netInclTotal, 0));

        decimal subtotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0, allocated = 0;
        for (var i = 0; i < lineCalc.Count; i++)
        {
            var (line, grossIncl, lineDiscIncl, netIncl) = lineCalc[i];
            decimal share;
            if (i == lineCalc.Count - 1) { share = RoundMoney(amountDiscount - allocated); }
            else
            {
                var ratio = netInclTotal > 0 ? netIncl / netInclTotal : 0;
                share = RoundMoney(amountDiscount * ratio);
            }
            allocated += share;

            var divisor = 1 + line.TaxRate / 100;
            var lineTotal = netIncl - share;                       // KDV dahil satır toplamı (esas tutar)
            var net = RoundMoney(lineTotal / divisor);             // KDV hariç net
            var grossExcl = RoundMoney(grossIncl / divisor);       // KDV hariç brüt
            line.TaxAmount = lineTotal - net;
            line.LineTotal = lineTotal;
            line.DiscountAmount = grossExcl - net;                 // KDV hariç toplam iskonto

            subtotal += grossExcl;
            discountTotal += line.DiscountAmount;
            taxTotal += line.TaxAmount;
            grandTotal += lineTotal;
        }

        invoice.AmountDiscount = amountDiscount;
        invoice.Subtotal = RoundMoney(subtotal);
        invoice.DiscountTotal = RoundMoney(discountTotal);
        invoice.TaxTotal = RoundMoney(taxTotal);
        invoice.GrandTotal = RoundMoney(grandTotal);
    }

    private static decimal RoundMoney(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
