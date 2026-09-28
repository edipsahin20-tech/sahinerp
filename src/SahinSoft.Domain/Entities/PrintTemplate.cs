using SahinSoft.Domain.Common;
using SahinSoft.Domain.Enums;

namespace SahinSoft.Domain.Entities;

// Çıktı Tasarımcısı şablonu (Edip: "Kullanıcı rapor tasarımını değiştirdiğinde kod değiştirmek
// gerekmemeli") - LayoutJson, PrintFieldRegistry'nin izin verdiği sabit element tipleriyle
// (text/staticText/divider/lineItemsTable/totalsBlock/paymentBreakdown/qrcode/barcode/image/
// spacer/cut) sıralı bir liste tutar; PrintRenderingService AYNI listeyi hem HTML önizleme hem
// ESC/POS bayt üretimi için yürütür. Versiyon geçmişi ayrı bir tabloya YAZILMAZ - EntityBase zaten
// her UPDATE'i AuditLog'a (eski/yeni JSON dahil) otomatik yazıyor, Version sadece ekranda "v3"
// gibi göstermek içindir.
public sealed class PrintTemplate : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public PrintTemplateType TemplateType { get; set; }
    public string LayoutJson { get; set; } = "[]";
    public int PaperWidthMm { get; set; } = 80;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int Version { get; set; } = 1;

    // Boşsa TÜM şubelerde geçerli varsayılan şablon; doluysa sadece o şubeye özel.
    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }

    public ICollection<Printer> Printers { get; set; } = new List<Printer>();
}
