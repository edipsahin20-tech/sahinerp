namespace SahinSoft.Web.Services;

// Ortak cari (ör. Trendyol tahsilat carisi) bakiyesini kaynak şubelere dağıtır.
// Kural (FIFO, kayıt sırası): tahsilat (alacak) önce kaydedilmiş en eski açık borçtan (satış) başlayarak kapatır; her borç
// satırı kendi kaynak şubesini taşır (CurrentAccountTransaction.OriginBranchId). Böylece bir
// tahsilat iki şubenin alacağını kapatıyorsa her şubenin kapanan kısmı ayrı görünür.
// Satış ikinci kez yazılmaz: bu sadece hesaplamadır, hiçbir kayıt üretmez.
public static class CariBranchAllocator
{
    public sealed record Row(DateTime Date, int Id, decimal Debit, decimal Credit, int? OriginBranchId);

    public sealed record BranchOpen(int? BranchId, decimal OpenAmount);

    public static IReadOnlyList<BranchOpen> OpenByBranch(IEnumerable<Row> rows)
    {
        // Açık borç parçaları (kalan tutar, kaynak şube). Sıra: tarih, sonra kayıt Id.
        var lots = new List<(int? Branch, decimal Remaining)>();
        decimal unmatchedCredit = 0;

        // Sıra KAYIT sırasına (Id) göre: tahsilatın fiş tarihi gün başıdır ve satıştan önce görünebilir.
        foreach (var row in rows.OrderBy(x => x.Id))
        {
            if (row.Debit > 0)
            {
                lots.Add((row.OriginBranchId, row.Debit));
            }

            var credit = row.Credit;
            for (var i = 0; i < lots.Count && credit > 0; i++)
            {
                var take = Math.Min(credit, lots[i].Remaining);
                if (take <= 0)
                {
                    continue;
                }

                lots[i] = (lots[i].Branch, lots[i].Remaining - take);
                credit -= take;
            }

            unmatchedCredit += credit;
        }

        // Kapanmamış (kalan > 0) borçları şubeye göre topla. Borçtan fazla tahsilat ayrı tutulur.
        var byBranch = lots
            .Where(x => x.Remaining > 0)
            .GroupBy(x => x.Branch)
            .Select(g => new BranchOpen(g.Key, g.Sum(x => x.Remaining)))
            .ToList();

        if (unmatchedCredit > 0)
        {
            byBranch.Add(new BranchOpen(null, -unmatchedCredit));
        }

        return byBranch;
    }
}

public static class ReceiptLinkResolver
{
    // Fiş numarası şubeler arası aynı olabilir (PSF.00003). Aday fişler arasından yalnızca KAYNAK
    // şubesi eşleşeni seçer. Kaynak şube yoksa ve aday birden fazlaysa tahmin yapılmaz (null).
    public static T? Pick<T>(IEnumerable<T> candidates, Func<T, int?> branchOf, int? originBranchId) where T : class
    {
        var list = candidates.ToList();
        if (originBranchId is { } origin)
        {
            return list.FirstOrDefault(x => branchOf(x) == origin);
        }

        return list.Count == 1 ? list[0] : null;
    }
}
