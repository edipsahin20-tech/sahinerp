using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Yazıcı Yönetimi (Edip: "Her şube/kasa için ayrı yazıcı tanımlanabilsin") - bir şubede birden
// fazla Printer satırı serbestçe olabilir, her biri kendi Role'üne (Adisyon/Mutfak/X/Z) ve
// isteğe bağlı bir PrintTemplate'e sahiptir. KitchenStationId SADECE Role=Mutfak için anlamlıdır
// - mevcut KitchenStation.PrinterName (salt metin) routing motoruna DOKUNMADAN, ek/opsiyonel bir
// bağlantı olarak eklenir (bkz. plan: "Mutfak Yönlendirmesi - Mevcut Motor Korunuyor").
public sealed class Printer : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public PrinterRole Role { get; set; }
    public PrinterConnectionType ConnectionType { get; set; } = PrinterConnectionType.WindowsSpooler;

    // WindowsSpooler için OS yazıcı kuyruk adı; NetworkRaw9100 için "ip:port" (örn. "192.168.1.50:9100").
    public string ConnectionAddress { get; set; } = string.Empty;

    // SADECE ConnectionType=WindowsSpooler için kullanılır - NetworkRaw9100 yazıcılara web
    // sunucusu doğrudan ham TCP soketle bağlanır (agent GEREKMEZ). WindowsSpooler'da ise fiziksel
    // USB yazıcının bağlı olduğu makinedeki SahinSoft.PrintAgent'ın yerel adresi (varsayılan
    // http://localhost:5058).
    public string? AgentBaseUrl { get; set; }

    public PrinterPaperWidth PaperWidth { get; set; } = PrinterPaperWidth.Mm80;
    public bool AutoCut { get; set; } = true;
    public int CopyCount { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;

    public int? PrintTemplateId { get; set; }
    public PrintTemplate? PrintTemplate { get; set; }

    // Sadece Role=Mutfak için anlamlı; opsiyonel (Edip'in mevcut KitchenStation.PrinterName
    // motoruna EK, mevcut motoru DEĞİŞTİRMEZ).
    public int? KitchenStationId { get; set; }
    public KitchenStation? KitchenStation { get; set; }

    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}
