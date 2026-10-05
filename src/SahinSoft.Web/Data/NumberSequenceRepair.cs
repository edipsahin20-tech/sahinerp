using Microsoft.EntityFrameworkCore;

namespace SahinSoft.Web.Data;

// Numara sayaçları (NumberSequences) mevcut kayıtlardan geride kalırsa (ör. sayaçlar sıfırlanıp ana veriler —
// cari, stok, hesap, personel — silinmeden kalırsa) yeni kayıt "bu kod zaten kullanılıyor" hatası verir.
// Açılışta, sayaç NextNumber'ı mevcut en büyük sonekin gerisindeyse YALNIZCA İLERİ alınır (asla geri/aşağı çekilmez;
// mevcut kayıtlara dokunulmaz, idempotenttir).
public static class NumberSequenceRepair
{
    public static async Task RunAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var codeSources = new Dictionary<string, Func<Task<List<string>>>>
        {
            ["CUSTOMER"] = () => db.Customers.AsNoTracking().Select(x => x.Code).ToListAsync(),
            ["STOCK"] = () => db.Products.AsNoTracking().Select(x => x.StockCode).ToListAsync(),
            ["QUOTE"] = () => db.Quotes.AsNoTracking().Select(x => x.QuoteNumber).ToListAsync(),
            ["EXPENSE"] = () => db.Expenses.AsNoTracking().Where(x => x.DocumentNumber != null).Select(x => x.DocumentNumber!).ToListAsync(),
            ["FINANCIAL_ACCOUNT_CASH"] = () => db.FinancialAccounts.AsNoTracking().Select(x => x.Code).ToListAsync(),
            ["FINANCIAL_ACCOUNT_BANK"] = () => db.FinancialAccounts.AsNoTracking().Select(x => x.Code).ToListAsync(),
            ["PERSONNEL"] = () => db.Users.AsNoTracking().Where(x => x.UserName != null).Select(x => x.UserName!).ToListAsync(),
        };

        var sequences = await db.NumberSequences.Where(x => codeSources.Keys.Contains(x.Key)).ToListAsync();
        var changed = false;
        foreach (var seq in sequences)
        {
            if (string.IsNullOrEmpty(seq.Prefix)) { continue; }
            var codes = await codeSources[seq.Key]();
            long max = 0;
            foreach (var code in codes)
            {
                if (code is null || !code.StartsWith(seq.Prefix, StringComparison.Ordinal)) { continue; }
                var suffix = code[seq.Prefix.Length..];
                if (suffix.Length > 0 && suffix.All(char.IsDigit) && long.TryParse(suffix, out var n) && n > max) { max = n; }
            }

            if (max >= seq.NextNumber)
            {
                logger.LogInformation("NumberSequence {Key}: NextNumber {Old} -> {New} (mevcut en büyük kod {Max}).", seq.Key, seq.NextNumber, max + 1, max);
                seq.NextNumber = max + 1;
                changed = true;
            }
        }

        if (changed) { await db.SaveChangesAsync(); }
    }
}
