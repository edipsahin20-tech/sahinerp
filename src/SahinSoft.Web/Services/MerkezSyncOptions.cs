namespace SahinSoft.Web.Services;

/// <summary>
/// appsettings.json / appsettings.Local.json "MerkezSync" bölümünden okunur.
/// Şube kurulumunda varsayılan Enabled=false'tur - bağlantı hiç yoksa şube tamamen
/// kendi başına çalışmaya devam eder. Bir şube merkeze bağlanmaya karar verdiğinde
/// (Kademe 1/2 geçişi) burası doldurulur, kod değişikliği gerekmez.
/// </summary>
public sealed class MerkezSyncOptions
{
    public const string SectionName = "MerkezSync";

    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int PollIntervalSeconds { get; set; } = 120;
}
