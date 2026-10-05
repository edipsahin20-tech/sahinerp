namespace SahinSoft.Web.Models;

// Talimat 1 (2026-09-06) madde 6-7 - "Veri Sağlığı / Mutabakat Merkezi": kullanıcı teknik tablo
// gezmek zorunda kalmadan, günlük satışların Yerel/Merkez/Muhasebe karşılıklarını tek ekranda
// görüp eksikleri güvenli şekilde onarabileceği tek, sade bir ekran.
public sealed class RestaurantDataHealthViewModel
{
    public DateOnly FilterDate { get; set; }
    public bool MerkezSyncEnabled { get; set; }

    // "fis" | "z" | "log" - Fiş Bazında Mutabakat / Z Bazında Mutabakat / Onarım Logu
    public string ActiveTab { get; set; } = "fis";
    public string? Search { get; set; }

    public int TotalCount { get; set; }
    public int MatchedCount { get; set; }
    public int PendingCount { get; set; }
    public int ErrorCount { get; set; }
    public int MissingCentralCount { get; set; }
    public int MissingAccountingCount { get; set; }

    public List<RestaurantDataHealthZSummaryRow> ZSummary { get; set; } = [];
    public List<RestaurantDataHealthRow> Rows { get; set; } = [];
    public List<RestaurantDataHealthAuditRow> RecentRepairs { get; set; } = [];

    public RestaurantDataHealthZDetail? ZDetail { get; set; }
    public RestaurantDataHealthRow? SelectedRow { get; set; }
    public List<RestaurantDataHealthAuditRow> SelectedRowAudits { get; set; } = [];
}

public sealed record RestaurantDataHealthZSummaryRow(
    int ZPeriodId,
    string ZNo,
    int BranchCount,
    int CentralCount,
    int AccountingCount,
    bool IsMismatched,
    decimal LocalTotal,
    decimal AccountingTotal,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc);

// Z Detayı paneli - seçilen Z'nin Yerel/Merkez/Muhasebe karşılaştırması ve bağlı fişler.
public sealed class RestaurantDataHealthZDetail
{
    public int ZPeriodId { get; set; }
    public string ZNo { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public DateTime OpenedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }
    public int LocalCount { get; set; }
    public decimal LocalTotal { get; set; }
    public decimal AccountingTotal { get; set; }
    public List<RestaurantDataHealthRow> Rows { get; set; } = [];
}

public sealed class RestaurantDataHealthRow
{
    public int RetailSaleId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string CheckNumber { get; set; } = string.Empty;
    public int? ZPeriodId { get; set; }
    public string? ZNo { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public decimal GrandTotal { get; set; }

    // "Tam" | "Eksik"
    public string LocalStatus { get; set; } = string.Empty;
    // "Tam" | "Bekliyor" | "Hatalı" | "Kapalı" (senkron kapalıyken N/A anlamında)
    public string CentralStatus { get; set; } = string.Empty;
    // "Tam" | "Eksik" | "Gerekmiyor" (İkram/0 tutar)
    public string AccountingStatus { get; set; } = string.Empty;

    // "YESIL" | "SARI" | "KIRMIZI"
    public string OverallStatus { get; set; } = string.Empty;
    public List<string> ErrorReasons { get; set; } = [];

    public bool CanRepairZReference { get; set; }
    public bool CanRetransmitToCentral { get; set; }
}

public sealed record RestaurantDataHealthAuditRow(
    DateTime CreatedAtUtc,
    string UserName,
    string Action,
    string EntityName,
    string? EntityId,
    string? OldValuesJson,
    string? NewValuesJson);
